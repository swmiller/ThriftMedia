using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreAdmins.ReactivateStoreAdmin;

public record ReactivateStoreAdminCommand(int AppUserId) : IRequest<Unit>;
