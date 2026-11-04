extern alias AdminApp;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using AdminApp::ThriftMedia.Admin.Auth;
using ThriftMedia.Contracts.Auth;
using ThriftMedia.Tests.Application;

namespace ThriftMedia.Tests.Api;

public class ApiTokenServiceTests
{
    private static readonly DateTime Now = new(2026, 11, 4, 12, 0, 0, DateTimeKind.Utc);
    private const string Key = "unit-test-signing-key-0123456789-abcdef";

    private static ApiTokenService Service() =>
        new(Options.Create(new TokenOptions { SigningKey = Key }), new FixedTimeProvider(Now));

    private static async Task<TokenValidationResult> Validate(string token, string audience) =>
        await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = ApiTokenDefaults.DefaultIssuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            ValidateLifetime = false
        });

    [Fact]
    public async Task UserToken_CarriesRoleStoreAndShortLifetime()
    {
        var token = Service().CreateUserToken(5, ApiTokenDefaults.RoleStoreAdmin, 10);

        var result = await Validate(token, ApiTokenDefaults.DefaultApiAudience);
        Assert.True(result.IsValid);
        var jwt = (JsonWebToken)result.SecurityToken;
        Assert.Equal("5", jwt.GetClaim(ApiTokenDefaults.ClaimSubject).Value);
        Assert.Equal("StoreAdmin", jwt.GetClaim(ApiTokenDefaults.ClaimRole).Value);
        Assert.Equal("10", jwt.GetClaim(ApiTokenDefaults.ClaimStoreId).Value);
        Assert.Equal(TimeSpan.FromMinutes(5), jwt.ValidTo - jwt.ValidFrom);
    }

    [Fact]
    public async Task SystemAdminToken_HasNoStoreClaim()
    {
        var token = Service().CreateUserToken(1, ApiTokenDefaults.RoleSystemAdmin, null);

        var jwt = (JsonWebToken)(await Validate(token, ApiTokenDefaults.DefaultApiAudience)).SecurityToken;
        Assert.False(jwt.TryGetClaim(ApiTokenDefaults.ClaimStoreId, out _));
    }

    [Fact]
    public async Task SignInToken_UsesSeparateAudienceAndIsVeryShortLived()
    {
        var token = Service().CreateSignInToken();

        Assert.True((await Validate(token, ApiTokenDefaults.DefaultSignInAudience)).IsValid);
        Assert.False((await Validate(token, ApiTokenDefaults.DefaultApiAudience)).IsValid);

        var jwt = new JsonWebToken(token);
        Assert.Equal(ApiTokenDefaults.PurposeSignIn, jwt.GetClaim(ApiTokenDefaults.ClaimPurpose).Value);
        Assert.Equal(TimeSpan.FromSeconds(60), jwt.ValidTo - jwt.ValidFrom);
    }

    [Fact]
    public async Task UserToken_IsNotAcceptedAsSignInToken()
    {
        var token = Service().CreateUserToken(1, ApiTokenDefaults.RoleSystemAdmin, null);
        Assert.False((await Validate(token, ApiTokenDefaults.DefaultSignInAudience)).IsValid);
    }
}
