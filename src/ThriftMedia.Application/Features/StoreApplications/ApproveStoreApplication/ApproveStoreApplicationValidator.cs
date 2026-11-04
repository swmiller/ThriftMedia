using FluentValidation;

namespace ThriftMedia.Application.Features.StoreApplications.ApproveStoreApplication;

public class ApproveStoreApplicationValidator : AbstractValidator<ApproveStoreApplicationCommand>
{
    public ApproveStoreApplicationValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0).WithMessage("A valid application ID is required.");
        RuleFor(x => x.Request.LicenseType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.IssuingAuthority).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.IssueDate).NotEmpty();
        RuleFor(x => x.Request.ExpirationDate)
            .GreaterThanOrEqualTo(x => x.Request.IssueDate)
            .When(x => x.Request.ExpirationDate.HasValue)
            .WithMessage("Expiration date cannot be earlier than the issue date.");
        RuleFor(x => x.Request.Notes).MaximumLength(1000);
    }
}
