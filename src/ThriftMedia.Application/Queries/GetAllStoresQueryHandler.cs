using ThriftMedia.Application.Repositories;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Queries;

/// <summary>
/// Handler for GetAllStoresQuery.
/// </summary>
public class GetAllStoresQueryHandler : IRequestHandler<GetAllStoresQuery, IEnumerable<StoreDto>>
{
    private readonly IStoreRepository _storeRepository;

    public GetAllStoresQueryHandler(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<IEnumerable<StoreDto>> Handle(GetAllStoresQuery request, CancellationToken cancellationToken)
    {
        var stores = await _storeRepository.GetAllAsync(cancellationToken);

        return stores.Select(store => new StoreDto(
            store.Id,
            store.Name,
            $"{store.Address?.Street}, {store.Address?.City}, {store.Address?.State} {store.Address?.ZipCode}"
        ));
    }
}
