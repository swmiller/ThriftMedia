using FluentValidation;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreAdmins.ReactivateStoreAdmin;

public class ReactivateStoreAdminCommandHandler(
    ICurrentUser currentUser,
    IValidator<ReactivateStoreAdminCommand> validator,
    IAppUserRepository users,
    IAccountNotifier notifier,
    TimeProvider timeProvider) : IRequestHandler<ReactivateStoreAdminCommand, Unit>
{
    public async Task<Unit> Handle(ReactivateStoreAdminCommand command, CancellationToken cancellationToken)
    {
        currentUser.RequireSystemAdmin();
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var user = await users.GetByIdAsync(command.AppUserId, cancellationToken)
            ?? throw new DomainNotFoundException($"User {command.AppUserId} was not found");

        user.Reactivate(timeProvider.GetUtcNow().UtcDateTime);
        await users.UpdateAsync(user, cancellationToken);

        await notifier.NotifyAsync(user.DomainEvents, cancellationToken);
        user.ClearDomainEvents();

        return Unit.Value;
    }
}
