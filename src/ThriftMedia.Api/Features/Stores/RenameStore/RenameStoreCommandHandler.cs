using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;
using DomainStore = ThriftMedia.Domain.Entities.Store;
using DomainAddress = ThriftMedia.Domain.ValueObjects.Address;
using PersistenceStore = ThriftMedia.Infrastructure.Persistence.Models.Store;

namespace ThriftMedia.Api.Features.Stores.RenameStore
{
    public record RenameStoreCommand(int Id, RenameStoreRequest Request) : IRequest<bool>;

    public class RenameStoreValidator : AbstractValidator<RenameStoreCommand>
    {
        public RenameStoreValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("A valid store ID is required.");

            RuleFor(x => x.Request.NewStoreName)
                .NotEmpty()
                .WithMessage("New store name is required.")
                .MaximumLength(100)
                .WithMessage("New store name must not exceed 100 characters.");
        }
    }

    public class RenameStoreCommandHandler : IRequestHandler<RenameStoreCommand, bool>
    {
        private readonly ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext _dbContext;
        private readonly IValidator<RenameStoreCommand> _validator;

        public RenameStoreCommandHandler(ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext dbContext, IValidator<RenameStoreCommand> validator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<bool> Handle(RenameStoreCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var persistenceStore = await _dbContext.Stores
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (persistenceStore is null)
                return false;

            var domainStore = ToDomain(persistenceStore);
            domainStore.Rename(command.Request.NewStoreName, "system", DateTime.UtcNow); // TODO: replace with authenticated user

            persistenceStore.StoreName = domainStore.StoreName;
            persistenceStore.UpdatedBy = domainStore.UpdatedBy;
            persistenceStore.UpdatedAt = domainStore.UpdatedAt;

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
    }
}
