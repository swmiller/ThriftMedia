using System.Net.Http.Headers;
using ThriftMedia.Contracts.Dto;

namespace ThriftMedia.Admin.Auth;

/// <summary>
/// Typed client for Api calls made on behalf of the signed-in user. The token is attached per request here
/// (not in a DelegatingHandler) because handlers run in the factory's scope, not the Blazor circuit's,
/// and so cannot see the circuit's authentication state.
/// </summary>
public sealed class AdminApiClient(HttpClient http, ApiTokenProvider tokenProvider)
{
    public async Task<IReadOnlyList<StoreApplicationDto>> GetPendingApplicationsAsync(CancellationToken cancellationToken)
    {
        using var request = await CreateAsync(HttpMethod.Get, "api/store-applications/pending", cancellationToken);
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<StoreApplicationDto>>(cancellationToken) ?? [];
    }

    private async Task<HttpRequestMessage> CreateAsync(HttpMethod method, string uri, CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetUserTokenAsync(cancellationToken)
            ?? throw new UnauthorizedAccessException("The current user cannot call the Api");

        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
