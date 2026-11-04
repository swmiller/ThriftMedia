namespace ThriftMedia.Contracts.Auth;

/// <summary>
/// Shared contract for the short-lived JWTs the Blazor apps mint for the API. Two token kinds exist:
/// an API token for a signed-in user, and a sign-in token that is accepted only by the sign-in
/// resolution endpoint (the user has no session yet when it is used).
/// </summary>
public static class ApiTokenDefaults
{
    public const string DefaultIssuer = "thriftmedia-apps";
    public const string DefaultApiAudience = "thriftmedia-api";
    public const string DefaultSignInAudience = "thriftmedia-api-signin";

    public const string ClaimSubject = "sub";
    public const string ClaimRole = "role";
    public const string ClaimStoreId = "storeId";
    public const string ClaimPurpose = "purpose";

    public const string PurposeSignIn = "signin";

    public const string RoleSystemAdmin = "SystemAdmin";
    public const string RoleStoreAdmin = "StoreAdmin";
}
