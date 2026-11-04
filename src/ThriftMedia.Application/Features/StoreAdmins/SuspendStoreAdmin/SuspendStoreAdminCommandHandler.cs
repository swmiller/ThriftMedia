using FluentValidation;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreAdmins.SuspendStoreAdmin;

public class SuspendStoreAdminCommandHandler(
    ICurrentUser currentUser,
    IValidator<SuspendStoreAdminCommand> validator,
    IAppUserRepository users,
    IAccountNotifier notifier,
    TimeProvider timeProvider) : IRequestHandler<SuspendStoreAdminCommand, Unit>
{
    public async Task<Unit> Handle(SuspendStoreAdminCommand command, CancellationToken cancellationToken)
    {
        currentUser.RequireSystemAdmin();
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await users.GetByIdAsync(command.AppUserId, cancellationToken)
            ?? throw new DomainNotFoundException($"User {command.AppUserId} was not found");

        user.Suspend(timeProvider.GetUtcNow().UtcDateTime);
        await users.UpdateAsync(user, cancellationToken);

        await notifier.NotifyAsync(user.DomainEvents, cancellationToken);
        user.ClearDomainEvents();

        return Unit.Value;
    }
}
