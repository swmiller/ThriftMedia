using ThriftMedia.Application.Features.StoreApplications.SubmitStoreApplication;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.StoreApplications.SubmitStoreApplication;

public static class SubmitStoreApplicationEndpoint
{
    public const string RateLimitPolicy = "store-application-submit";

    /// <summary>
    /// Public "Apply" endpoint. Always answers 202 for a valid request, whether or not the email is
    /// already registered, so it cannot be used to discover accounts. Rate limited per client.
    /// </summary>
    public static IEndpointRouteBuilder MapSubmitStoreApplicationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/store-applications", async (SubmitStoreApplicationRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new SubmitStoreApplicationCommand(request), cancellationToken);
            return Results.Accepted();
        })
        .WithName("SubmitStoreApplication")
        .WithTags("StoreApplications")
        .AllowAnonymous()
        .RequireRateLimiting(RateLimitPolicy)
        .Produces(StatusCodes.Status202Accepted)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
