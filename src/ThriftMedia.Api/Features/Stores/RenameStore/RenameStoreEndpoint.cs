using FluentValidation;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.RenameStore
{
    public static class RenameStoreEndpoint
    {
        public static IEndpointRouteBuilder MapRenameStoreEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("/stores/{id:int}/name", async (int id, RenameStoreRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                try
                {
                    var found = await mediator.Send(new RenameStoreCommand(id, request), cancellationToken);

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
            .WithName("RenameStore")
            .WithTags("Stores")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            return app;
        }
    }
}
