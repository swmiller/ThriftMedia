using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.GetAllStores
{
    public static class GetAllStoresEndpoint
    {
        public static IEndpointRouteBuilder MapGetAllStoresEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/stores", async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetAllStoresQuery(), cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetAllStores")
            .WithTags("Stores")
            .Produces<IEnumerable<StoreDto>>(StatusCodes.Status200OK);

            return app;
        }
    }
}
