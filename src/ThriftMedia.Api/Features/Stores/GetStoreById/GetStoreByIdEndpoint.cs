using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.GetStoreById
{
    public static class GetStoreByIdEndpoint
    {
        public static IEndpointRouteBuilder MapGetStoreByIdEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/stores/{id:int}", async (int id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetStoreByIdQuery(id), cancellationToken);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            })
                .WithName("GetStoreById")
                .WithTags("Stores")
                .Produces<StoreDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}
