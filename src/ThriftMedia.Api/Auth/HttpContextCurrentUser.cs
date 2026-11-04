using System.Globalization;
using System.Security.Claims;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Contracts.Auth;
using ThriftMedia.Domain.Enums;

namespace ThriftMedia.Api.Auth;

/// <summary>Reads the caller from the validated API token's claims.</summary>
internal sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int? AppUserId => IsAuthenticated ? ParseInt(ApiTokenDefaults.ClaimSubject) : null;

    public AppUserRole? Role =>
        IsAuthenticated
        && Enum.TryParse<AppUserRole>(Principal!.FindFirst(ApiTokenDefaults.ClaimRole)?.Value, out var role)
        && Enum.IsDefined(role)
            ? role
            : null;

    public int? StoreId => IsAuthenticated ? ParseInt(ApiTokenDefaults.ClaimStoreId) : null;

    private int? ParseInt(string claimType) =>
        int.TryParse(Principal!.FindFirst(claimType)?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var v) ? v : null;
}
