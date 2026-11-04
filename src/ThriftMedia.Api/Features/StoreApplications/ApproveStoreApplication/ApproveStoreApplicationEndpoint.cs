using ThriftMedia.Api.Auth;
using ThriftMedia.Application.Features.StoreApplications.ApproveStoreApplication;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.StoreApplications.ApproveStoreApplication;

public static class ApproveStoreApplicationEndpoint
{
    public static IEndpointRouteBuilder MapApproveStoreApplicationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/store-applications/{id:int}/approve", async (int id, ApproveStoreApplicationRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var storeId = await mediator.Send(new ApproveStoreApplicationCommand(id, request), cancellationToken);
            return Results.Ok(new { storeId });
        })
        .WithName("ApproveStoreApplication")
        .WithTags("StoreApplications")
        .RequireAuthorization(AuthPolicies.SystemAdmin)
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
