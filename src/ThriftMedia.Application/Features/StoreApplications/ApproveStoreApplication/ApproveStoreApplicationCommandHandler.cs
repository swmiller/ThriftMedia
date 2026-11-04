using FluentValidation;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Exceptions;
using ThriftMedia.Domain.ValueObjects;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application.Features.StoreApplications.ApproveStoreApplication;

/// <summary>
/// System Admin approves a pending application: creates the Store Admin account and the store
/// (using the license details the admin verified) and marks the application approved, atomically.
/// The applicant is then notified by email.
/// </summary>
public class ApproveStoreApplicationCommandHandler(
    ICurrentUser currentUser,
    IValidator<ApproveStoreApplicationCommand> validator,
    IStoreApplicationRepository applications,
    IAppUserRepository users,
    IStoreRepository stores,
    IUnitOfWork unitOfWork,
    IAccountNotifier notifier,
    TimeProvider timeProvider) : IRequestHandler<ApproveStoreApplicationCommand, int>
{
    public async Task<int> Handle(ApproveStoreApplicationCommand command, CancellationToken cancellationToken)
    {
        var reviewerId = currentUser.RequireSystemAdmin();
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var application = await applications.GetByIdAsync(command.ApplicationId, cancellationToken)
            ?? throw new DomainNotFoundException($"Store application {command.ApplicationId} was not found");

        if (await users.GetByEmailAsync(application.Email, cancellationToken) is not null)
            throw new DomainValidationException("An account with this email address already exists");

        var request = command.Request;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var storeId = 0;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var storeAdmin = AppUser.CreateStoreAdmin(application.Email, application.OwnerFullName, now);
            var storeAdminId = await users.AddAsync(storeAdmin, ct);

            var (firstName, lastName) = SplitName(application.OwnerFullName);
            var store = Store.Create(
                application.StoreName,
                application.LicenseNumber,
                request.LicenseType,
                request.IssuingAuthority,
                request.IssueDate,
                request.ExpirationDate,
                "Verified",
                Address.Create(application.Address1, application.Address2, application.City,
                    application.ProvinceState, application.PostalCode, application.Country),
                storeAdminId,
                $"app-user:{reviewerId}",
                now,
                phoneNumber: application.PhoneNumber,
                websiteUrl: application.WebsiteUrl,
                ownerFirstName: firstName,
                ownerLastName: lastName,
                ownerPhoneNumber: application.PhoneNumber,
                ownerEmail: application.Email);
            storeId = await stores.AddAsync(store, ct);

            application.Approve(reviewerId, storeAdminId, request.Notes, now);
            await applications.UpdateAsync(application, ct);
        }, cancellationToken);

        // Only after commit: a failed email must not undo the approval.
        await notifier.NotifyAsync(application.DomainEvents, cancellationToken);
        application.ClearDomainEvents();

        return storeId;
    }

    /// <summary>Last word is the last name; everything before it is the first name.</summary>
    private static (string? First, string? Last) SplitName(string fullName)
    {
        var name = fullName.Trim();
        var lastSpace = name.LastIndexOf(' ');
        return lastSpace < 0
            ? (name, null)
            : (name[..lastSpace].Trim(), name[(lastSpace + 1)..].Trim());
    }
}
