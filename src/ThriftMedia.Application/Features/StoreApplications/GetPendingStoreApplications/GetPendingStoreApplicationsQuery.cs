using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreApplications.GetPendingStoreApplications;

public record GetPendingStoreApplicationsQuery : IRequest<IReadOnlyList<StoreApplicationDto>>;
