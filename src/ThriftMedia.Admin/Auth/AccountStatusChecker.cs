using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Contracts.Requests;

namespace ThriftMedia.Admin.Auth;

/// <summary>
/// Re-resolves the signed-in user against the Api (cached ~1 minute) so suspension, removal from the
/// allowlist or a role change takes effect within minutes rather than at cookie expiry.
/// </summary>
public sealed class AccountStatusChecker(SignInClient signInClient, IMemoryCache cache, ILogger<AccountStatusChecker> logger)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

    /// <summary>The user's current sign-in result, or null if the Api could not be reached (nothing is cached then).</summary>
    public async Task<SignInResultDto?> RecheckAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var provider = user.FindFirstValue(AdminClaims.Provider);
        var providerSub = user.FindFirstValue(AdminClaims.ProviderSub);
        if (provider is null || providerSub is null) return null;

        var key = $"account-status:{provider}:{providerSub}";
        if (cache.TryGetValue(key, out SignInResultDto? cached)) return cached;

        try
        {
            var result = await signInClient.ResolveAsync(
                new ResolveSignInRequest(provider, providerSub, user.FindFirstValue(AdminClaims.Email), true,
                    user.FindFirstValue(AdminClaims.Name), null),
                cancellationToken);
            cache.Set(key, result, CacheDuration);
            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Could not re-check account status; the Api is unreachable");
            return null;
        }
    }
}
