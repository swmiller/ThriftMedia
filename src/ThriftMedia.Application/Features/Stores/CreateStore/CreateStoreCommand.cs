using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.Stores.CreateStore;

public record CreateStoreCommand(CreateStoreRequest request) : IRequest<int>;
