using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.Stores.GetStoreById;

public record GetStoreByIdQuery(int Id) : IRequest<StoreDto?>;
