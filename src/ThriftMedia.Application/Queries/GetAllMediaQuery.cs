using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Queries;

/// <summary>
/// Query to get all media items.
/// </summary>
public record GetAllMediaQuery : IRequest<IEnumerable<MediaDto>>;
