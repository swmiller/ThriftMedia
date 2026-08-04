using FluentValidation;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Mediator;
using DomainStore = ThriftMedia.Domain.Entities.Store;
using DomainAddress = ThriftMedia.Domain.ValueObjects.Address;
using PersistenceStore = ThriftMedia.Infrastructure.Persistence.Models.Store;

namespace ThriftMedia.Api.Features.Stores.CreateStore
{
    public record CreateStoreCommand(CreateStoreRequest request) : IRequest<int>;

    public class CreateStoreValidator : AbstractValidator<CreateStoreCommand>
    {
        // TODO: revisit validation rules based on changing business requirements and constraints
        public CreateStoreValidator() 
        {
            RuleFor(x => x.request.StoreName)
                .NotEmpty()
                .WithMessage("Store name is required.")
                .MaximumLength(100)
                .WithMessage("Store name must not exceed 100 characters.");

            RuleFor(x => x.request.Address1)
                .NotEmpty()
                .WithMessage("Address1 is required.")
                .MaximumLength(200)
                .WithMessage("Address1 must not exceed 200 characters.");

            RuleFor(x => x.request.City)
                .NotEmpty()
                .WithMessage("City is required.")
                .MaximumLength(100)
                .WithMessage("City must not exceed 100 characters.");

            RuleFor(x => x.request.PostalCode)
                .NotEmpty()
                .WithMessage("Postal code is required.");

            RuleFor(x => x.request.LicenseNumber)
                .NotEmpty()
                .WithMessage("License number is required.")
                .MaximumLength(50)
                .WithMessage("License number must not exceed 50 characters.");

            RuleFor(x => x.request.LicenseType)
                .NotEmpty()
                .WithMessage("License type is required.")
                .MaximumLength(50)
                .WithMessage("License type must not exceed 50 characters.");

            RuleFor(x => x.request.IssueingAuthority)
                .NotEmpty()
                .WithMessage("Issuing authority is required.")
                .MaximumLength(100)
                .WithMessage("Issuing authority must not exceed 100 characters.");

            RuleFor(x => x.request.LicenseStatus)
                .NotEmpty()
                .WithMessage("License status is required.")
                .MaximumLength(50)
                .WithMessage("License status must not exceed 50 characters.");

            RuleFor(x => x.request.AppUserId)
                .GreaterThan(0)
                .WithMessage("App user ID must be a positive integer.");
        }
    }

    public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, int>
    {
        private readonly ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext _dbContext;
        private readonly IValidator<CreateStoreCommand> _validator;

        public CreateStoreCommandHandler(ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext dbContext, IValidator<CreateStoreCommand> validator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<int> Handle(CreateStoreCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var req = command.request;

            var address = DomainAddress.Create(
                req.Address1,
                req.Address2,
                req.City,
                req.ProvinceState,
                req.PostalCode,
                req.Country ?? string.Empty);

            var domainStore = DomainStore.Create(
                req.StoreName,
                req.LicenseNumber,
                req.LicenseType,
                req.IssueingAuthority,
                req.IssueDate,
                req.ExpirationDate,
                req.LicenseStatus,
                address,
                req.AppUserId,
                "system", // TODO: replace with authenticated user
                DateTime.UtcNow,
                phoneNumber: req.PhoneNumber,
                websiteUrl: req.WebsiteUrl,
                ownerFirstName: req.OwnerFirstName,
                ownerLastName: req.OwnerLastName,
                ownerPhoneNumber: req.OwnerPhoneNumber,
                ownerEmail: req.OwnerEmail);

            var persistenceStore = new PersistenceStore
            {
                StoreName = domainStore.StoreName,
                PhoneNumber = domainStore.PhoneNumber,
                WebsiteUrl = domainStore.WebsiteUrl,
                IsActive = domainStore.IsActive,
                IsSuspended = domainStore.IsSuspended,
                OwnerFirstName = domainStore.OwnerFirstName,
                OwnerLastName = domainStore.OwnerLastName,
                OwnerPhoneNumber = domainStore.OwnerPhoneNumber,
                OwnerEmail = domainStore.OwnerEmail,
                LicenseNumber = domainStore.LicenseNumber,
                LicenseType = domainStore.LicenseType,
                IssueingAuthority = domainStore.IssueingAuthority,
                IssueDate = domainStore.IssueDate,
                ExpirationDate = domainStore.ExpirationDate,
                LicenseStatus = domainStore.LicenseStatus,
                Address1 = domainStore.Address.Line1,
                Address2 = domainStore.Address.Line2 ?? string.Empty,
                City = domainStore.Address.City,
                PostalCode = domainStore.Address.PostalCode,
                Country = domainStore.Address.Country,
                ProvinceState = domainStore.Address.ProvinceState,
                AppUserId = domainStore.AppUserId,
                CreatedBy = domainStore.CreatedBy,
                CreatedAt = domainStore.CreatedAt,
            };

            _dbContext.Stores.Add(persistenceStore);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return persistenceStore.Id;
        }
    }
}
