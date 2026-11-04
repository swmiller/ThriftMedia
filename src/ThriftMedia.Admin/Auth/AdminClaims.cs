namespace ThriftMedia.Admin.Auth;

/// <summary>Claim types in the Admin app's session cookie (the Api's token claims are in <c>ApiTokenDefaults</c>).</summary>
public static class AdminClaims
{
    public const string AppUserId = "sub";
    public const string Role = "role";
    public const string StoreId = "storeId";
    public const string Name = "name";
    public const string Email = "email";
    public const string Provider = "idp";
    public const string ProviderSub = "idsub";
}

public static class AdminPolicies
{
    public const string SystemAdmin = "SystemAdmin";
    public const string StoreMember = "StoreMember";
}
