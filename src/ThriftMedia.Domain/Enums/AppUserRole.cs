namespace ThriftMedia.Domain.Enums;

/// <summary>Persisted as <c>auth."AppUsers"."Role"</c>. Values must not change.</summary>
public enum AppUserRole
{
    /// <summary>The single platform administrator (the "System Admin").</summary>
    SystemAdmin = 0,

    /// <summary>The one sign-in for a store's pages. Owns exactly one store.</summary>
    StoreAdmin = 1
}
