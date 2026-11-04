using ThriftMedia.Api.Auth;
using ThriftMedia.Application.Features.StoreApplications.GetPendingStoreApplications;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.StoreApplications.GetPendingStoreApplications;

public static class GetPendingStoreApplicationsEndpoint
{
    public static IEndpointRouteBuilder MapGetPendingStoreApplicationsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/store-applications/pending", async (IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new GetPendingStoreApplicationsQuery(), cancellationToken)))
            .WithName("GetPendingStoreApplications")
            .WithTags("StoreApplications")
            .RequireAuthorization(AuthPolicies.SystemAdmin)
            .Produces<IReadOnlyList<StoreApplicationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }
}
