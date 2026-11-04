using FluentValidation;
using Microsoft.Extensions.Logging;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Entities;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreApplications.SubmitStoreApplication;

/// <summary>
/// Public endpoint. A duplicate (pending application or existing account for the email) is
/// silently ignored so the response never reveals whether an email is already registered.
/// </summary>
public class SubmitStoreApplicationCommandHandler(
    IValidator<SubmitStoreApplicationCommand> validator,
    IStoreApplicationRepository applications,
    IAppUserRepository users,
    TimeProvider timeProvider,
    ILogger<SubmitStoreApplicationCommandHandler> logger) : IRequestHandler<SubmitStoreApplicationCommand, Unit>
{
    public async Task<Unit> Handle(SubmitStoreApplicationCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var r = command.Request;
        var application = StoreApplication.Submit(
            r.StoreName, r.OwnerFullName, r.Email, r.PhoneNumber,
            r.Address1, r.Address2, r.City, r.ProvinceState, r.PostalCode, r.Country,
            r.WebsiteUrl, r.Description,
            r.LicenseNumber, r.LicenseImageRef, r.StoreFrontImageRef,
            timeProvider.GetUtcNow().UtcDateTime);

        if (await applications.HasPendingForEmailAsync(application.Email, cancellationToken)
            || await users.GetByEmailAsync(application.Email, cancellationToken) is not null)
        {
            logger.LogInformation("Ignored duplicate store application submission");
            return Unit.Value;
        }

        var id = await applications.AddAsync(application, cancellationToken);
        logger.LogInformation("Store application {ApplicationId} submitted", id);
        return Unit.Value;
    }
}
