using FluentValidation;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreApplications.RejectStoreApplication;

public class RejectStoreApplicationCommandHandler(
    ICurrentUser currentUser,
    IValidator<RejectStoreApplicationCommand> validator,
    IStoreApplicationRepository applications,
    IAccountNotifier notifier,
    TimeProvider timeProvider) : IRequestHandler<RejectStoreApplicationCommand, Unit>
{
    public async Task<Unit> Handle(RejectStoreApplicationCommand command, CancellationToken cancellationToken)
    {
        var reviewerId = currentUser.RequireSystemAdmin();
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var application = await applications.GetByIdAsync(command.ApplicationId, cancellationToken)
            ?? throw new DomainNotFoundException($"Store application {command.ApplicationId} was not found");

        application.Reject(reviewerId, command.Request.Reason, timeProvider.GetUtcNow().UtcDateTime);
        await applications.UpdateAsync(application, cancellationToken);

        await notifier.NotifyAsync(application.DomainEvents, cancellationToken);
        application.ClearDomainEvents();

        return Unit.Value;
    }
}
