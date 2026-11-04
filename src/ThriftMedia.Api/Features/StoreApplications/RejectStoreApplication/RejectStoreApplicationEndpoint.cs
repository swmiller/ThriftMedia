using ThriftMedia.Api.Auth;
using ThriftMedia.Application.Features.StoreApplications.RejectStoreApplication;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.StoreApplications.RejectStoreApplication;

public static class RejectStoreApplicationEndpoint
{
    public static IEndpointRouteBuilder MapRejectStoreApplicationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/store-applications/{id:int}/reject", async (int id, RejectStoreApplicationRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RejectStoreApplicationCommand(id, request), cancellationToken);
            return Results.NoContent();
        })
        .WithName("RejectStoreApplication")
        .WithTags("StoreApplications")
        .RequireAuthorization(AuthPolicies.SystemAdmin)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
