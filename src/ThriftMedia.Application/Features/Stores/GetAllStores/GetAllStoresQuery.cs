using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.Stores.GetAllStores;

public class GetAllStoresQuery : IRequest<IEnumerable<StoreDto>>;
