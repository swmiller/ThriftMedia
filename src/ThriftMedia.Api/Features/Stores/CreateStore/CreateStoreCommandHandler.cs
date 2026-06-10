using FluentValidation;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Infrastructure.Persistence.Models;
using ThriftMedia.Mediator;

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
        private readonly ThriftMediaDbContext _dbContext;
        private readonly IValidator<CreateStoreCommand> _validator;

        public CreateStoreCommandHandler(ThriftMediaDbContext dbContext, IValidator<CreateStoreCommand> validator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<int> Handle(CreateStoreCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var createStoreRequest = command.request;

            var store = new Store() 
            {
                StoreName = createStoreRequest.StoreName,
                PhoneNumber = createStoreRequest.PhoneNumber,
                WebsiteUrl = createStoreRequest.WebsiteUrl,
                OwnerFirstName = createStoreRequest.OwnerFirstName,
                OwnerLastName = createStoreRequest.OwnerLastName,
                OwnerPhoneNumber = createStoreRequest.OwnerPhoneNumber,
                OwnerEmail = createStoreRequest.OwnerEmail,
                LicenseNumber = createStoreRequest.LicenseNumber,
                LicenseType = createStoreRequest.LicenseType,
                IssueingAuthority = createStoreRequest.IssueingAuthority,
                IssueDate = createStoreRequest.IssueDate,
                ExpirationDate = createStoreRequest.ExpirationDate,
                LicenseStatus = createStoreRequest.LicenseStatus,
                Address1 = createStoreRequest.Address1,
                Address2 = createStoreRequest.Address2,
                City = createStoreRequest.City,
                PostalCode = createStoreRequest.PostalCode,
                Country = createStoreRequest.Country,
                ProvinceState = createStoreRequest.ProvinceState,
                AppUserId = createStoreRequest.AppUserId,
                IsActive = true,
                IsSuspended = false,
                CreatedBy = "system", // TODO: replace with authenticated user
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Stores.Add(store);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return store.Id;
        }
    }
}
