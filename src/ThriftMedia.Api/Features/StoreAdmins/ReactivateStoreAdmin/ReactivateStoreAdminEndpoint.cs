using ThriftMedia.Api.Auth;
using ThriftMedia.Application.Features.StoreAdmins.ReactivateStoreAdmin;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.StoreAdmins.ReactivateStoreAdmin;

public static class ReactivateStoreAdminEndpoint
{
    public static IEndpointRouteBuilder MapReactivateStoreAdminEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/store-admins/{id:int}/reactivate", async (int id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new ReactivateStoreAdminCommand(id), cancellationToken);
            return Results.NoContent();
        })
        .WithName("ReactivateStoreAdmin")
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
