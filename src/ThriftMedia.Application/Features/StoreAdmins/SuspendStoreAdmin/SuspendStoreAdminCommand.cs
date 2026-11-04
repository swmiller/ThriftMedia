using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreAdmins.SuspendStoreAdmin;

public record SuspendStoreAdminCommand(int AppUserId) : IRequest<Unit>;
