using FluentValidation;

namespace ThriftMedia.Application.Features.StoreApplications.RejectStoreApplication;

public class RejectStoreApplicationValidator : AbstractValidator<RejectStoreApplicationCommand>
{
    public RejectStoreApplicationValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0).WithMessage("A valid application ID is required.");
        RuleFor(x => x.Request.Reason).MaximumLength(1000);
    }
}
