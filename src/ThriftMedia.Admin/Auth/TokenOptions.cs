using System.ComponentModel.DataAnnotations;
using ThriftMedia.Contracts.Auth;

namespace ThriftMedia.Admin.Auth;

/// <summary>
/// Bound from the "Auth" section, the same one the Api reads. The signing key is shared with the Api
/// (supplied as an Aspire secret parameter / user-secret, never committed).
/// </summary>
public sealed class TokenOptions
{
    public const string SectionName = "Auth";

    [Required, MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = ApiTokenDefaults.DefaultIssuer;
    public string ApiAudience { get; set; } = ApiTokenDefaults.DefaultApiAudience;
    public string SignInAudience { get; set; } = ApiTokenDefaults.DefaultSignInAudience;
}
