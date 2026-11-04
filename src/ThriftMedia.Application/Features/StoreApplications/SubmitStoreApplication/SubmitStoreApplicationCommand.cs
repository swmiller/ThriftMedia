using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreApplications.SubmitStoreApplication;

public record SubmitStoreApplicationCommand(SubmitStoreApplicationRequest Request) : IRequest<Unit>;
