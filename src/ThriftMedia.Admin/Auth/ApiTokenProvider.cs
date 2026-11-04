using Microsoft.AspNetCore.Components.Authorization;
using ThriftMedia.Contracts.Dto;

namespace ThriftMedia.Admin.Auth;

/// <summary>
/// Produces an Api token for the current user. Reads the user from <see cref="AuthenticationStateProvider"/>
/// (not HttpContext) because Blazor Server circuits outlive the request that created them.
/// </summary>
public sealed class ApiTokenProvider(
    AuthenticationStateProvider authenticationState,
    AccountStatusChecker statusChecker,
    ApiTokenService tokens)
{
    /// <summary>A fresh token, or null if the user is anonymous or no longer allowed to sign in.</summary>
    public async Task<string?> GetUserTokenAsync(CancellationToken cancellationToken)
    {
        var state = await authenticationState.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated != true) return null;

        var status = await statusChecker.RecheckAsync(state.User, cancellationToken);
        if (status is not { Outcome: SignInOutcome.Succeeded, AppUserId: { } id, Role: { } role })
            return null;

        return tokens.CreateUserToken(id, role, status.StoreId);
    }
}
