using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.Stores.RenameStore;

public record RenameStoreCommand(int Id, RenameStoreRequest Request) : IRequest<bool>;
