using System.Net.Http.Json;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Tests.Api;

public class ApiAuthorizationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly DateTime Now = new(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);

    private const string Pending = "/api/store-applications/pending";

    private static SubmitStoreApplicationRequest ValidSubmission(string email) => new(
        "Second Chapter Books", "Pat Q Owner", email, "555-0100",
        "1 Main St", null, "Springfield", "IL", "62701", null,
        null, "Used books and vinyl", "LIC-123", "uploads/license.pdf", null);

    private StoreApplication SeedPendingApplication(string email)
    {
        var r = ValidSubmission(email);
        return factory.Applications.Seed(StoreApplication.Submit(
            r.StoreName, r.OwnerFullName, r.Email, r.PhoneNumber, r.Address1, r.Address2, r.City,
            r.ProvinceState, r.PostalCode, r.Country, r.WebsiteUrl, r.Description,
            r.LicenseNumber, r.LicenseImageRef, r.StoreFrontImageRef, Now));
    }

    // ---- authentication: who gets in at all ----

    [Fact]
    public async Task NoToken_IsUnauthorized()
    {
        var response = await factory.ClientWith(null).GetAsync(Pending);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GarbageToken_IsUnauthorized()
    {
        var response = await factory.ClientWith("not-a-jwt").GetAsync(Pending);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExpiredToken_IsUnauthorized()
    {
        var response = await factory.ClientWith(ApiFactory.ExpiredToken()).GetAsync(Pending);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenSignedWithWrongKey_IsUnauthorized()
    {
        var response = await factory.ClientWith(ApiFactory.TokenSignedWithWrongKey()).GetAsync(Pending);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WrongAudience_IsUnauthorized()
    {
        var token = ApiFactory.Custom("thriftmedia-apps", "some-other-api");
        var response = await factory.ClientWith(token).GetAsync(Pending);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WrongIssuer_IsUnauthorized()
    {
        var token = ApiFactory.Custom("someone-else", "thriftmedia-api");
        var response = await factory.ClientWith(token).GetAsync(Pending);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnlistedRoute_RequiresAuthentication()
    {
        // The existing Store endpoints have no explicit policy: the deny-by-default fallback applies.
        var response = await factory.ClientWith(null).GetAsync("/api/stores");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---- authorization: roles ----

    [Fact]
    public async Task StoreAdmin_CannotListPendingApplications()
    {
        var response = await factory.ClientWith(ApiFactory.StoreAdminToken()).GetAsync(Pending);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SystemAdmin_CanListPendingApplications()
    {
        SeedPendingApplication("listed@example.com");

        var response = await factory.ClientWith(ApiFactory.SystemAdminToken()).GetAsync(Pending);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<StoreApplicationDto>>();
        Assert.Contains(list!, a => a.Email == "listed@example.com");
    }

    [Theory]
    [InlineData("/api/store-applications/1/approve")]
    [InlineData("/api/store-applications/1/reject")]
    [InlineData("/api/store-admins/1/suspend")]
    [InlineData("/api/store-admins/1/reactivate")]
    public async Task StoreAdmin_CannotUseSystemAdminCommands(string url)
    {
        var response = await factory.ClientWith(ApiFactory.StoreAdminToken()).PostAsJsonAsync(url, new { });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/store-applications/1/approve")]
    [InlineData("/api/store-applications/1/reject")]
    [InlineData("/api/store-admins/1/suspend")]
    [InlineData("/api/store-admins/1/reactivate")]
    public async Task Anonymous_CannotUseSystemAdminCommands(string url)
    {
        var response = await factory.ClientWith(null).PostAsJsonAsync(url, new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SystemAdminToken_WithoutSystemAdminRole_ClaimsAreNotEnough()
    {
        // A store admin token claiming a store must not satisfy the SystemAdmin policy.
        var response = await factory.ClientWith(ApiFactory.StoreAdminToken(id: 1)).GetAsync(Pending);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---- sign-in token separation ----

    [Fact]
    public async Task SignInEndpoint_RejectsApiTokens()
    {
        var response = await factory.ClientWith(ApiFactory.SystemAdminToken())
            .PostAsJsonAsync("/api/auth/sign-in", new ResolveSignInRequest("Google", "s", "a@b.c", true, null, null));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ApiEndpoints_RejectSignInToken()
    {
        var response = await factory.ClientWith(ApiFactory.SignInToken()).GetAsync(Pending);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SignInEndpoint_WithoutToken_IsUnauthorized()
    {
        var response = await factory.ClientWith(null)
            .PostAsJsonAsync("/api/auth/sign-in", new ResolveSignInRequest("Google", "s", "a@b.c", true, null, null));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SignIn_ApprovedStoreAdmin_ReturnsRoleAndLinksAccount()
    {
        factory.Users.Seed(AppUser.CreateStoreAdmin("approved@example.com", null, Now), 77);

        var response = await factory.ClientWith(ApiFactory.SignInToken())
            .PostAsJsonAsync("/api/auth/sign-in", new ResolveSignInRequest("Google", "g-77", "approved@example.com", true, "Pat", null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SignInResultDto>();
        Assert.Equal(SignInOutcome.Succeeded, result!.Outcome);
        Assert.Equal(77, result.AppUserId);
        Assert.Equal("StoreAdmin", result.Role);
    }

    [Fact]
    public async Task SignIn_UnknownEmail_ReturnsNotRegisteredWithoutError()
    {
        var response = await factory.ClientWith(ApiFactory.SignInToken())
            .PostAsJsonAsync("/api/auth/sign-in", new ResolveSignInRequest("Google", "g-x", "nobody@example.com", true, null, null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SignInResultDto>();
        Assert.Equal(SignInOutcome.NotRegistered, result!.Outcome);
    }

    // ---- public submit ----

    [Fact]
    public async Task Submit_IsAnonymousAndAlways202()
    {
        var client = factory.ClientWith(null);
        var first = await client.PostAsJsonAsync("/api/store-applications", ValidSubmission("dup@example.com"));
        var duplicate = await client.PostAsJsonAsync("/api/store-applications", ValidSubmission("dup@example.com"));

        Assert.Equal(HttpStatusCode.Accepted, first.StatusCode);
        Assert.Equal(HttpStatusCode.Accepted, duplicate.StatusCode); // no email enumeration
    }

    [Fact]
    public async Task Submit_InvalidRequest_Returns400WithErrors()
    {
        var response = await factory.ClientWith(null).PostAsJsonAsync("/api/store-applications", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---- end to end through the real pipeline ----

    [Fact]
    public async Task SystemAdmin_CanApproveApplication_AndStoreAdminCanThenSignIn()
    {
        var application = SeedPendingApplication("flow@example.com");

        var approve = await factory.ClientWith(ApiFactory.SystemAdminToken(id: 1)).PostAsJsonAsync(
            $"/api/store-applications/{application.Id}/approve",
            new ApproveStoreApplicationRequest("Retail", "State of Illinois", new DateTime(2025, 1, 1), null, "ok"));
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);

        var signIn = await factory.ClientWith(ApiFactory.SignInToken())
            .PostAsJsonAsync("/api/auth/sign-in", new ResolveSignInRequest("Google", "g-flow", "flow@example.com", true, null, null));
        var result = await signIn.Content.ReadFromJsonAsync<SignInResultDto>();

        Assert.Equal(SignInOutcome.Succeeded, result!.Outcome);
        Assert.NotNull(result.StoreId);
    }

    [Fact]
    public async Task Approve_UnknownApplication_Returns404()
    {
        var response = await factory.ClientWith(ApiFactory.SystemAdminToken()).PostAsJsonAsync(
            "/api/store-applications/9999/approve",
            new ApproveStoreApplicationRequest("Retail", "State of Illinois", new DateTime(2025, 1, 1), null, null));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Approve_InvalidBody_Returns400()
    {
        var response = await factory.ClientWith(ApiFactory.SystemAdminToken()).PostAsJsonAsync(
            "/api/store-applications/1/approve", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SystemAdmin_CanSuspendAndReactivateStoreAdmin()
    {
        factory.Users.Seed(AppUser.CreateStoreAdmin("suspend@example.com", null, Now), 88);
        // Make the account active so it can be suspended.
        var user = (await factory.Users.GetByIdAsync(88, CancellationToken.None))!;
        user.LinkIdentity("Google", "g-88", null, Now);
        await factory.Users.UpdateAsync(user, CancellationToken.None);

        var client = factory.ClientWith(ApiFactory.SystemAdminToken());
        var suspend = await client.PostAsync("/api/store-admins/88/suspend", null);
        Assert.Equal(HttpStatusCode.NoContent, suspend.StatusCode);

        var blocked = await factory.ClientWith(ApiFactory.SignInToken())
            .PostAsJsonAsync("/api/auth/sign-in", new ResolveSignInRequest("Google", "g-88", "suspend@example.com", true, null, null));
        Assert.Equal(SignInOutcome.Suspended, (await blocked.Content.ReadFromJsonAsync<SignInResultDto>())!.Outcome);

        var reactivate = await client.PostAsync("/api/store-admins/88/reactivate", null);
        Assert.Equal(HttpStatusCode.NoContent, reactivate.StatusCode);
    }

    // ---- System Admin allowlist from configuration ----

    [Fact]
    public async Task SystemAdmin_IsProvisionedOnlyForConfiguredProviderAndEmail()
    {
        var client = factory.ClientWith(ApiFactory.SignInToken());

        var wrongProvider = await client.PostAsJsonAsync("/api/auth/sign-in",
            new ResolveSignInRequest("Google", "g-admin", ApiFactory.AdminEmail, true, null, null));
        Assert.Equal(SignInOutcome.NotRegistered, (await wrongProvider.Content.ReadFromJsonAsync<SignInResultDto>())!.Outcome);

        var allowed = await client.PostAsJsonAsync("/api/auth/sign-in",
            new ResolveSignInRequest("Microsoft", "ms-admin", ApiFactory.AdminEmail, true, "Admin", null));
        var result = await allowed.Content.ReadFromJsonAsync<SignInResultDto>();
        Assert.Equal(SignInOutcome.Succeeded, result!.Outcome);
        Assert.Equal("SystemAdmin", result.Role);
    }
}
