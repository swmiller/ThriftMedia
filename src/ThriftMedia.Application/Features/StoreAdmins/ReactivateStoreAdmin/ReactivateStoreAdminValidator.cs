using FluentValidation;

namespace ThriftMedia.Application.Features.StoreAdmins.ReactivateStoreAdmin;

public class ReactivateStoreAdminValidator : AbstractValidator<ReactivateStoreAdminCommand>
{
    public ReactivateStoreAdminValidator()
    {
        RuleFor(x => x.AppUserId).GreaterThan(0).WithMessage("A valid user ID is required.");
    }
}
