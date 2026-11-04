using ThriftMedia.Application.Abstractions;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreApplications.GetPendingStoreApplications;

public class GetPendingStoreApplicationsQueryHandler(
    ICurrentUser currentUser,
    IStoreApplicationRepository applications) : IRequestHandler<GetPendingStoreApplicationsQuery, IReadOnlyList<StoreApplicationDto>>
{
    public async Task<IReadOnlyList<StoreApplicationDto>> Handle(GetPendingStoreApplicationsQuery query, CancellationToken cancellationToken)
    {
        currentUser.RequireSystemAdmin();

        var pending = await applications.GetPendingAsync(cancellationToken);
        return pending.Select(a => a.ToDto()).ToList();
    }
}
