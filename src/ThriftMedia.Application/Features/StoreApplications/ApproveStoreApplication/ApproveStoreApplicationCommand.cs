using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreApplications.ApproveStoreApplication;

/// <summary>Returns the id of the newly created store.</summary>
public record ApproveStoreApplicationCommand(int ApplicationId, ApproveStoreApplicationRequest Request) : IRequest<int>;
