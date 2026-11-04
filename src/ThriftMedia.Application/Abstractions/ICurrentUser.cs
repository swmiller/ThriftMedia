using ThriftMedia.Domain.Enums;

namespace ThriftMedia.Application.Abstractions;

/// <summary>
/// The authenticated caller, read from the validated API token. Handlers use it for
/// defense-in-depth checks and store scoping; the API still enforces policies at the boundary.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>The caller's <c>auth."AppUsers"."Id"</c>.</summary>
    int? AppUserId { get; }

    AppUserRole? Role { get; }

    /// <summary>The caller's store (Store Admins only).</summary>
    int? StoreId { get; }

    bool IsSystemAdmin => IsAuthenticated && Role == AppUserRole.SystemAdmin;
}
