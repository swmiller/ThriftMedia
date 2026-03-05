using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Queries;

/// <summary>
/// Query to get all stores.
/// </summary>
public record GetAllStoresQuery : IRequest<IEnumerable<StoreDto>>;
