using FluentValidation;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.CreateStore
{
    public static class CreateStoreEndpoint
    {
        public static IEndpointRouteBuilder MapCreateStoreEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/stores", async (CreateStoreRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                try
                {
                    var id = await mediator.Send(new CreateStoreCommand(request), cancellationToken);
                    return Results.Created($"/api/stores/{id}", id);
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                    
                    return Results.ValidationProblem(errors);
                }
            })
            .WithName("CreateStore")
            .WithTags("Stores")
            .Produces<int>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            return app;
        }
    }
}
