using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Enums;
using ThriftMedia.Domain.Events;
using ThriftMedia.Domain.Exceptions;

namespace ThriftMedia.Tests.Domain;

public class AppUserTests
{
    private static readonly DateTime Now = new(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

    private static AppUser ActiveStoreAdmin(int id = 7)
    {
        var user = AppUser.CreateStoreAdmin("owner@example.com", "Owner", Now);
        user.LinkIdentity("Google", "sub-1", null, Now);
        return AppUser.Rehydrate(id, user.Provider, user.ProviderSub, user.Email, user.DisplayName,
            user.Role, user.Status, user.CreatedAtUtc, user.LastSeenAtUtc);
    }

    [Fact]
    public void CreateStoreAdmin_StartsApprovedWithoutLinkedIdentity()
    {
        var user = AppUser.CreateStoreAdmin("  Owner@Example.com ", null, Now);

        Assert.Equal(AppUserRole.StoreAdmin, user.Role);
        Assert.Equal(AppUserStatus.Approved, user.Status);
        Assert.False(user.HasLinkedIdentity);
        Assert.Equal("Owner@Example.com", user.Email);
        Assert.True(user.CanSignIn);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-at-sign")]
    [InlineData("two@@example.com")]
    [InlineData("a b@example.com")]
    [InlineData("user@nodot")]
    [InlineData("@example.com")]
    public void CreateStoreAdmin_RejectsInvalidEmail(string email)
    {
        Assert.Throws<DomainValidationException>(() => AppUser.CreateStoreAdmin(email, null, Now));
    }

    [Fact]
    public void CreateSystemAdmin_IsActiveAndLinked()
    {
        var user = AppUser.CreateSystemAdmin("Google", "sub-admin", "me@example.com", "Me", Now);

        Assert.Equal(AppUserRole.SystemAdmin, user.Role);
        Assert.Equal(AppUserStatus.Active, user.Status);
        Assert.True(user.HasLinkedIdentity);
    }

    [Fact]
    public void EmailMatches_IgnoresCaseAndWhitespace()
    {
        var user = AppUser.CreateStoreAdmin("Owner@Example.com", null, Now);

        Assert.True(user.EmailMatches(" owner@example.COM "));
        Assert.False(user.EmailMatches("other@example.com"));
        Assert.False(user.EmailMatches(null));
    }

    [Fact]
    public void LinkIdentity_ActivatesApprovedAccount()
    {
        var user = AppUser.CreateStoreAdmin("owner@example.com", null, Now);
        var later = Now.AddMinutes(5);

        user.LinkIdentity("Microsoft", "sub-9", "Owner Name", later);

        Assert.Equal(AppUserStatus.Active, user.Status);
        Assert.Equal("Microsoft", user.Provider);
        Assert.Equal("sub-9", user.ProviderSub);
        Assert.Equal("Owner Name", user.DisplayName);
        Assert.Equal(later, user.LastSeenAtUtc);
    }

    [Fact]
    public void LinkIdentity_RejectsSecondLink()
    {
        var user = ActiveStoreAdmin();

        Assert.Throws<DomainValidationException>(() => user.LinkIdentity("Google", "other-sub", null, Now));
    }

    [Fact]
    public void RecordSignIn_RequiresLinkedIdentity()
    {
        var user = AppUser.CreateStoreAdmin("owner@example.com", null, Now);

        Assert.Throws<DomainValidationException>(() => user.RecordSignIn(Now));
    }

    [Fact]
    public void Suspend_BlocksSignInAndRaisesEvent()
    {
        var user = ActiveStoreAdmin(id: 7);

        user.Suspend(Now);

        Assert.Equal(AppUserStatus.Suspended, user.Status);
        Assert.False(user.CanSignIn);
        var evt = Assert.IsType<AppUserSuspendedEvent>(Assert.Single(user.DomainEvents));
        Assert.Equal(7, evt.AppUserId);
        Assert.Equal("owner@example.com", evt.Email);
        Assert.Throws<DomainValidationException>(() => user.RecordSignIn(Now));
    }

    [Fact]
    public void Suspend_RejectsNonActiveAccount()
    {
        var approved = AppUser.CreateStoreAdmin("owner@example.com", null, Now);

        Assert.Throws<DomainValidationException>(() => approved.Suspend(Now));
    }

    [Fact]
    public void Suspend_RejectsSystemAdmin()
    {
        var admin = AppUser.CreateSystemAdmin("Google", "sub-admin", "me@example.com", null, Now);

        Assert.Throws<DomainValidationException>(() => admin.Suspend(Now));
    }

    [Fact]
    public void LinkIdentity_RejectsSuspendedAccount()
    {
        var user = ActiveStoreAdmin();
        user.Suspend(Now);

        Assert.Throws<DomainValidationException>(() => user.LinkIdentity("Google", "x", null, Now));
    }

    [Fact]
    public void Reactivate_RestoresSuspendedAccount()
    {
        var user = ActiveStoreAdmin();
        user.Suspend(Now);
        user.ClearDomainEvents();

        user.Reactivate(Now);

        Assert.Equal(AppUserStatus.Active, user.Status);
        Assert.True(user.CanSignIn);
        Assert.IsType<AppUserReactivatedEvent>(Assert.Single(user.DomainEvents));
    }

    [Fact]
    public void Reactivate_RejectsAccountThatIsNotSuspended()
    {
        var user = ActiveStoreAdmin();

        Assert.Throws<DomainValidationException>(() => user.Reactivate(Now));
    }
}
