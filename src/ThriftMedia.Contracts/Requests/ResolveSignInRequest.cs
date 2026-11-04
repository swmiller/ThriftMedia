namespace ThriftMedia.Contracts.Requests;

/// <summary>
/// Claims asserted by the external identity provider after a successful federated sign-in.
/// Sent by the trusted Blazor app to decide whether the person may use ThriftMedia.
/// </summary>
/// <param name="Provider">Authentication scheme name, e.g. "Google" or "Microsoft".</param>
/// <param name="ProviderSub">The provider's stable subject identifier.</param>
/// <param name="Email">Email asserted by the provider.</param>
/// <param name="EmailVerified">Whether the provider asserted the email as verified.</param>
/// <param name="DisplayName">Optional display name.</param>
/// <param name="MfaAsserted">True/false when the provider reported whether MFA was used (amr claim); null when it did not say.</param>
public record ResolveSignInRequest(
    string Provider,
    string ProviderSub,
    string? Email,
    bool EmailVerified,
    string? DisplayName,
    bool? MfaAsserted);
