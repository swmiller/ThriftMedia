using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ThriftMedia.Application.Queries;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Get all media
        endpoints.MapGet("/media", async (IMediator mediator) =>
        {
            var media = await mediator.Send(new GetAllMediaQuery());
            return Results.Ok(media);
        });


        // Get media by ID - TODO: Implement GetMediaByIdQuery
        endpoints.MapGet("/media/{id:guid}", (Guid id) =>
        {
            // Placeholder - implement GetMediaByIdQuery handler
            return Results.NotFound();
        });

        // Add new media - TODO: Implement CreateMediaCommand
        endpoints.MapPost("/media", (MediaDto media) =>
        {
            // Placeholder - implement CreateMediaCommand handler
            return Results.StatusCode(501); // Not Implemented
        });

        return endpoints;
    }
}
