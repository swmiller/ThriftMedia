using FluentValidation;

namespace ThriftMedia.Application.Features.Stores.CreateStore;

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
