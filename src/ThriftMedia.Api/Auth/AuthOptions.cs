using System.ComponentModel.DataAnnotations;
using ThriftMedia.Contracts.Auth;

namespace ThriftMedia.Api.Auth;

/// <summary>Bound from the "Auth" configuration section. Secrets come from user-secrets / Aspire parameters, never source control.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Symmetric HS256 key shared with the apps that mint tokens. At least 32 characters.</summary>
    [Required, MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = ApiTokenDefaults.DefaultIssuer;
    public string ApiAudience { get; set; } = ApiTokenDefaults.DefaultApiAudience;
    public string SignInAudience { get; set; } = ApiTokenDefaults.DefaultSignInAudience;

    /// <summary>Provider of the single System Admin account, e.g. "Microsoft". Empty disables System Admin sign-in.</summary>
    public string SystemAdminProvider { get; set; } = string.Empty;

    /// <summary>Email of the single System Admin. Supplied via configuration only; never committed.</summary>
    public string SystemAdminEmail { get; set; } = string.Empty;
}
