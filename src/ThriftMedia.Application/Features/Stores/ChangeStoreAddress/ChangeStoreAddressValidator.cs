using FluentValidation;

namespace ThriftMedia.Application.Features.Stores.ChangeStoreAddress;

public class ChangeStoreAddressValidator : AbstractValidator<ChangeStoreAddressCommand>
{
    public ChangeStoreAddressValidator()
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
