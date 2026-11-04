using FluentValidation;

namespace ThriftMedia.Application.Features.StoreApplications.SubmitStoreApplication;

public class SubmitStoreApplicationValidator : AbstractValidator<SubmitStoreApplicationCommand>
{
    public SubmitStoreApplicationValidator()
    {
        RuleFor(x => x.Request.StoreName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.OwnerFullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.Request.PhoneNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Address1).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Address2).MaximumLength(150);
        RuleFor(x => x.Request.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.ProvinceState).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.Country).MaximumLength(100);
        RuleFor(x => x.Request.WebsiteUrl).MaximumLength(255);
        RuleFor(x => x.Request.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Request.LicenseNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.LicenseImageRef)
            .NotEmpty().WithMessage("A business license image is required.")
            .MaximumLength(500);
        RuleFor(x => x.Request.StoreFrontImageRef).MaximumLength(500);
    }
}
