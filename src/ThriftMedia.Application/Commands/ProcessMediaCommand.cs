using MediatR;

namespace ThriftMedia.Application.Commands;

/// <summary>
/// Command to process a media item through the ingestion pipeline.
/// </summary>
public record ProcessMediaCommand(Guid MediaId, int StoreId, Uri ImageUri) : IRequest<bool>;
