using FluentValidation;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.ChangeStoreAddress
{
    public static class ChangeStoreAddressEndpoint
    {
        public static IEndpointRouteBuilder MapChangeStoreAddressEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/stores/{id:int}/address", async (int id, ChangeStoreAddressRequest request, IMediator mediator, CancellationToken cancellationToken) =>
            {
                try
                {
                    var found = await mediator.Send(new ChangeStoreAddressCommand(id, request), cancellationToken);

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
                catch (DomainValidationException ex)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        [nameof(ChangeStoreAddressRequest)] = [ex.Message]
                    });
                }
            })
            .WithName("ChangeStoreAddress")
            .WithTags("Stores")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            return app;
        }
    }
}
