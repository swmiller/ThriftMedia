using FluentValidation;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.DeleteStore
{
    public static class DeleteStoreEndpoint
    {
        public static IEndpointRouteBuilder MapDeleteStoreEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/stores/{id:int}", async (int id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                try
                {
                    var found = await mediator.Send(new DeleteStoreCommand(id), cancellationToken);

                    return found
                        ? Results.NoContent()
                        : Results.NotFound();
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                    return Results.ValidationProblem(errors);
                }
            })
            .WithName("DeleteStore")
            .WithTags("Stores")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            return app;
        }
    }
}
