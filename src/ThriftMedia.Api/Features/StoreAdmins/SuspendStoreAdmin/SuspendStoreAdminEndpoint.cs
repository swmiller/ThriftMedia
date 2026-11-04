using ThriftMedia.Api.Auth;
using ThriftMedia.Application.Features.StoreAdmins.SuspendStoreAdmin;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.StoreAdmins.SuspendStoreAdmin;

public static class SuspendStoreAdminEndpoint
{
    public static IEndpointRouteBuilder MapSuspendStoreAdminEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/store-admins/{id:int}/suspend", async (int id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new SuspendStoreAdminCommand(id), cancellationToken);
            return Results.NoContent();
        })
        .WithName("SuspendStoreAdmin")
        .WithTags("StoreAdmins")
        .RequireAuthorization(AuthPolicies.SystemAdmin)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
