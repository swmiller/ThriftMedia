using ThriftMedia.Application.Features.Auth.ResolveSignIn;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Enums;

namespace ThriftMedia.Tests.Application;

public class ResolveSignInTests
{
    private const string AdminEmail = "admin@example.com";
    private static readonly DateTime Now = new(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeAppUserRepository _users = new();
    private readonly FakeStoreRepository _stores = new();

    private ResolveSignInCommandHandler Handler() => new(
        new ResolveSignInValidator(), _users, _stores, new FakeAllowlist("Microsoft", AdminEmail), new FixedTimeProvider(Now));

    private Task<SignInResultDto> SignIn(
        string provider = "Google", string sub = "sub-1", string? email = "owner@example.com",
        bool emailVerified = true, bool? mfa = null) =>
        Handler().Handle(
            new ResolveSignInCommand(new ResolveSignInRequest(provider, sub, email, emailVerified, "Name", mfa)),
            CancellationToken.None);

    private AppUser SeedApprovedStoreAdmin(string email = "owner@example.com", int id = 5) =>
        _users.Seed(AppUser.CreateStoreAdmin(email, null, Now), id);

    [Fact]
    public async Task FirstSignIn_LinksApprovedStoreAdminByVerifiedEmail()
    {
        SeedApprovedStoreAdmin();

        var result = await SignIn(email: "Owner@Example.com");

        Assert.Equal(SignInOutcome.Succeeded, result.Outcome);
        Assert.Equal(5, result.AppUserId);
        Assert.Equal("StoreAdmin", result.Role);
        var stored = await _users.GetByIdAsync(5, CancellationToken.None);
        Assert.Equal(AppUserStatus.Active, stored!.Status);
        Assert.Equal("sub-1", stored.ProviderSub);
    }

    [Fact]
    public async Task SignIn_ReturnsStoreIdForStoreAdmin()
    {
        SeedApprovedStoreAdmin();
        await _stores.AddAsync(StoreFor(appUserId: 5), CancellationToken.None);

        var result = await SignIn();

        Assert.Equal(50, result.StoreId);
    }

    [Fact]
    public async Task FirstSignIn_WithUnverifiedEmail_IsRejectedAndDoesNotLink()
    {
        SeedApprovedStoreAdmin();

        var result = await SignIn(emailVerified: false);

        Assert.Equal(SignInOutcome.EmailNotVerified, result.Outcome);
        Assert.False((await _users.GetByIdAsync(5, CancellationToken.None))!.HasLinkedIdentity);
    }

    [Fact]
    public async Task FirstSignIn_WithUnknownEmail_IsNotRegistered()
    {
        var result = await SignIn(email: "stranger@example.com");

        Assert.Equal(SignInOutcome.NotRegistered, result.Outcome);
        Assert.Empty(_users.All);
    }

    [Fact]
    public async Task FirstSignIn_ForAccountAlreadyLinkedToAnotherIdentity_IsNotRegistered()
    {
        var user = SeedApprovedStoreAdmin();
        user.LinkIdentity("Google", "original-sub", null, Now);

        var result = await SignIn(provider: "Microsoft", sub: "other-sub");

        Assert.Equal(SignInOutcome.NotRegistered, result.Outcome);
    }

    [Fact]
    public async Task KnownIdentity_SignsInAndRecordsLastSeen()
    {
        var user = SeedApprovedStoreAdmin();
        user.LinkIdentity("Google", "sub-1", null, Now.AddDays(-1));

        var result = await SignIn();

        Assert.Equal(SignInOutcome.Succeeded, result.Outcome);
        Assert.Equal(Now, (await _users.GetByIdAsync(5, CancellationToken.None))!.LastSeenAtUtc);
    }

    [Fact]
    public async Task SuspendedUser_IsBlocked()
    {
        var user = SeedApprovedStoreAdmin();
        user.LinkIdentity("Google", "sub-1", null, Now);
        user.Suspend(Now);

        var result = await SignIn();

        Assert.Equal(SignInOutcome.Suspended, result.Outcome);
    }

    [Fact]
    public async Task AllowlistedAdmin_IsProvisionedOnFirstSignIn()
    {
        var result = await SignIn(provider: "Microsoft", sub: "admin-sub", email: AdminEmail, mfa: true);

        Assert.Equal(SignInOutcome.Succeeded, result.Outcome);
        Assert.Equal("SystemAdmin", result.Role);
        Assert.Null(result.StoreId);
        Assert.Single(_users.All, u => u.Role == AppUserRole.SystemAdmin);
    }

    [Fact]
    public async Task AllowlistedAdmin_ProvisioningAllowedWhenProviderDoesNotReportMfa()
    {
        var result = await SignIn(provider: "Microsoft", sub: "admin-sub", email: AdminEmail, mfa: null);

        Assert.Equal(SignInOutcome.Succeeded, result.Outcome);
    }

    [Fact]
    public async Task AllowlistedAdmin_WhenProviderReportsNoMfa_IsRejected()
    {
        var result = await SignIn(provider: "Microsoft", sub: "admin-sub", email: AdminEmail, mfa: false);

        Assert.Equal(SignInOutcome.MfaRequired, result.Outcome);
        Assert.Empty(_users.All);
    }

    [Fact]
    public async Task AdminEmailOnWrongProvider_CannotBecomeAdmin()
    {
        var result = await SignIn(provider: "Google", sub: "g-sub", email: AdminEmail);

        Assert.Equal(SignInOutcome.NotRegistered, result.Outcome);
        Assert.DoesNotContain(_users.All, u => u.Role == AppUserRole.SystemAdmin);
    }

    [Fact]
    public async Task SecondIdentityForAdminEmail_IsNotAccepted()
    {
        _users.Seed(AppUser.CreateSystemAdmin("Microsoft", "admin-sub", AdminEmail, null, Now), 1);

        var result = await SignIn(provider: "Microsoft", sub: "different-sub", email: AdminEmail);

        Assert.Equal(SignInOutcome.NotRegistered, result.Outcome);
        Assert.Single(_users.All);
    }

    [Fact]
    public async Task ExistingAdmin_LosesAccessWhenRemovedFromAllowlist()
    {
        _users.Seed(AppUser.CreateSystemAdmin("Microsoft", "admin-sub", "old-admin@example.com", null, Now), 1);

        var result = await SignIn(provider: "Microsoft", sub: "admin-sub", email: "old-admin@example.com");

        Assert.Equal(SignInOutcome.NotRegistered, result.Outcome);
    }

    [Fact]
    public async Task ExistingAdmin_WhenProviderReportsNoMfa_IsRejected()
    {
        _users.Seed(AppUser.CreateSystemAdmin("Microsoft", "admin-sub", AdminEmail, null, Now), 1);

        var result = await SignIn(provider: "Microsoft", sub: "admin-sub", email: AdminEmail, mfa: false);

        Assert.Equal(SignInOutcome.MfaRequired, result.Outcome);
    }

    [Fact]
    public async Task InvalidRequest_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => SignIn(provider: "", sub: "x"));
    }

    private static Store StoreFor(int appUserId) => Store.Create(
        "Test Store", "LIC-1", "Retail", "State", Now.AddYears(-1), null, "Verified",
        ThriftMedia.Domain.ValueObjects.Address.Create("1 Main", null, "City", "IL", "62701", "United States"),
        appUserId, "test", Now);
}
