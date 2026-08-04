using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;
using DomainStore = ThriftMedia.Domain.Entities.Store;
using DomainAddress = ThriftMedia.Domain.ValueObjects.Address;
using PersistenceStore = ThriftMedia.Infrastructure.Persistence.Models.Store;

namespace ThriftMedia.Api.Features.Stores.ChangeStoreAddress
{
    public record ChangeStoreAddressCommand(int Id, ChangeStoreAddressRequest Request) : IRequest<bool>;

    public class ChangeStoreAddressCommandValidator : AbstractValidator<ChangeStoreAddressCommand>
    {
        public ChangeStoreAddressCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("A valid store ID is required.");

            RuleFor(x => x.Request.Address1)
                .NotEmpty()
                .WithMessage("Address line 1 is required.")
                .MaximumLength(150)
                .WithMessage("Address line 1 must not exceed 150 characters.");

            RuleFor(x => x.Request.Address2)
                .MaximumLength(150)
                .WithMessage("Address line 2 must not exceed 150 characters.")
                .When(x => x.Request.Address2 is not null);

            RuleFor(x => x.Request.City)
                .NotEmpty()
                .WithMessage("City is required.")
                .MaximumLength(100)
                .WithMessage("City must not exceed 100 characters.");

            RuleFor(x => x.Request.PostalCode)
                .NotEmpty()
                .WithMessage("Postal code is required.")
                .MaximumLength(20)
                .WithMessage("Post code must not exceed 20 characters.");

            RuleFor(x => x.Request.ProvinceState)
                .MaximumLength(50)
                .WithMessage("Province/State must not exceed 50 characters.");

            RuleFor(x => x.Request.Country)
                .MaximumLength(100)
                .WithMessage("Country must not exceed 100 characters.");
        }
    }

    public class ChangeStoreAddressCommandHandler : IRequestHandler<ChangeStoreAddressCommand, bool>
    {
        private const string SystemUser = "system";

        private readonly ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext _dbContext;
        private readonly IValidator<ChangeStoreAddressCommand> _validator;

        public ChangeStoreAddressCommandHandler(ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext dbContext, IValidator<ChangeStoreAddressCommand> validator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<bool> Handle(ChangeStoreAddressCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var persistenceStore = await _dbContext.Stores
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (persistenceStore is null)
                return false;

            var domainStore = ToDomain(persistenceStore);

            var newAddress = DomainAddress.Create(
                command.Request.Address1,
                command.Request.Address2,
                command.Request.City,
                command.Request.ProvinceState,
                command.Request.PostalCode,
                command.Request.Country ?? string.Empty);

            domainStore.ChangeAddress(newAddress, SystemUser, DateTime.UtcNow);

            ApplyDomainAddressChanges(persistenceStore, domainStore);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static DomainStore ToDomain(PersistenceStore store)
        {
            var address = DomainAddress.Create(
                store.Address1, store.Address2, store.City,
                store.ProvinceState, store.PostalCode, store.Country ?? string.Empty);

            return DomainStore.Rehydrate(
                store.Id, store.StoreName, store.PhoneNumber, store.WebsiteUrl,
                store.IsActive, store.IsSuspended,
                store.OwnerFirstName, store.OwnerLastName, store.OwnerPhoneNumber, store.OwnerEmail,
                store.LicenseNumber, store.LicenseType, store.IssueingAuthority,
                store.IssueDate, store.ExpirationDate, store.LicenseStatus,
                address,
                store.CreatedBy, store.CreatedAt,
                store.UpdatedBy, store.UpdatedAt,
                store.AppUserId);
        }

        private static void ApplyDomainAddressChanges(PersistenceStore persistenceStore, DomainStore domainStore)
        {
            persistenceStore.Address1 = domainStore.Address.Line1;
            persistenceStore.Address2 = domainStore.Address.Line2 ?? string.Empty;
            persistenceStore.City = domainStore.Address.City;
            persistenceStore.PostalCode = domainStore.Address.PostalCode;
            persistenceStore.ProvinceState = domainStore.Address.ProvinceState;
            persistenceStore.Country = domainStore.Address.Country;
            persistenceStore.UpdatedBy = domainStore.UpdatedBy;
            persistenceStore.UpdatedAt = domainStore.UpdatedAt;
        }
    }
}
