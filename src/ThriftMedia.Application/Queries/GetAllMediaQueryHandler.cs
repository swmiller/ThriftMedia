using ThriftMedia.Application.Repositories;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Queries;

/// <summary>
/// Handler for GetAllMediaQuery.
/// </summary>
public class GetAllMediaQueryHandler : IRequestHandler<GetAllMediaQuery, IEnumerable<MediaDto>>
{
    private readonly IMediaRepository _mediaRepository;

    public GetAllMediaQueryHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public async Task<IEnumerable<MediaDto>> Handle(GetAllMediaQuery request, CancellationToken cancellationToken)
    {
        var mediaItems = await _mediaRepository.GetAllAsync(cancellationToken);

        return mediaItems.Select(media => new MediaDto(
            media.Id,
            media.Title ?? "Unknown",
            media.Type?.ToString() ?? "Unknown",
            media.Description ?? string.Empty
        ));
    }
}
