using ThriftMedia.Api.Auth;
using ThriftMedia.Application.Features.Auth.ResolveSignIn;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Auth.ResolveSignIn;

public static class ResolveSignInEndpoint
{
    /// <summary>
    /// Called by the trusted Admin app after the federated login completes. Accepts only the app-signed
    /// sign-in token. The outcome (including refusals) is in the body so the app can show the right message.
    /// </summary>
    public static IEndpointRouteBuilder MapResolveSignInEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/sign-in", async (ResolveSignInRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new ResolveSignInCommand(request), cancellationToken)))
            .WithName("ResolveSignIn")
            .WithTags("Auth")
            .RequireAuthorization(AuthPolicies.SignIn)
            .Produces<SignInResultDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
