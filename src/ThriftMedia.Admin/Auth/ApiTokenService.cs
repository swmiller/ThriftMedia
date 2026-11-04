using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ThriftMedia.Contracts.Auth;

namespace ThriftMedia.Admin.Auth;

/// <summary>Mints the short-lived JWTs the Api validates. No refresh tokens: a new token is minted per use.</summary>
public sealed class ApiTokenService(IOptions<TokenOptions> options, TimeProvider timeProvider)
{
    private static readonly TimeSpan UserTokenLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SignInTokenLifetime = TimeSpan.FromSeconds(60);

    private readonly JsonWebTokenHandler _handler = new();

    /// <summary>Accepted only by the Api's sign-in resolution endpoint.</summary>
    public string CreateSignInToken() => Create(
        options.Value.SignInAudience,
        SignInTokenLifetime,
        new Dictionary<string, object>
        {
            [ApiTokenDefaults.ClaimSubject] = "thriftmedia-admin-app",
            [ApiTokenDefaults.ClaimPurpose] = ApiTokenDefaults.PurposeSignIn
        });

    /// <summary>An API token for a signed-in user, carrying the role and (for Store Admins) the store id.</summary>
    public string CreateUserToken(int appUserId, string role, int? storeId)
    {
        var claims = new Dictionary<string, object>
        {
            [ApiTokenDefaults.ClaimSubject] = appUserId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            [ApiTokenDefaults.ClaimRole] = role
        };
        if (storeId is not null)
            claims[ApiTokenDefaults.ClaimStoreId] = storeId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return Create(options.Value.ApiAudience, UserTokenLifetime, claims);
    }

    private string Create(string audience, TimeSpan lifetime, Dictionary<string, object> claims)
    {
        var o = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = o.Issuer,
            Audience = audience,
            Claims = claims,
            NotBefore = now,
            IssuedAt = now,
            Expires = now.Add(lifetime),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.SigningKey)), SecurityAlgorithms.HmacSha256)
        });
    }
}
