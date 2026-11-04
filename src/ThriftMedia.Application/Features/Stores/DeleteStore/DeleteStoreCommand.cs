using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.Stores.DeleteStore;

public record DeleteStoreCommand(int Id) : IRequest<bool>;
