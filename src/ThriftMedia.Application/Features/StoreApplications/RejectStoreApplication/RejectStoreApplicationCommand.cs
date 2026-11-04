using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreApplications.RejectStoreApplication;

public record RejectStoreApplicationCommand(int ApplicationId, RejectStoreApplicationRequest Request) : IRequest<Unit>;
