using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.Stores.ChangeStoreAddress;

public record ChangeStoreAddressCommand(int Id, ChangeStoreAddressRequest Request) : IRequest<bool>;
