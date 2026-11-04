using System.Net.Http.Headers;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Contracts.Requests;

namespace ThriftMedia.Admin.Auth;

/// <summary>Calls the Api's sign-in resolution endpoint, authenticated with an app-signed sign-in token.</summary>
public sealed class SignInClient(HttpClient http, ApiTokenService tokens)
{
    public async Task<SignInResultDto> ResolveAsync(ResolveSignInRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/auth/sign-in")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.CreateSignInToken());

        using var response = await http.SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SignInResultDto>(cancellationToken)
            ?? throw new InvalidOperationException("Empty sign-in response");
    }
}
