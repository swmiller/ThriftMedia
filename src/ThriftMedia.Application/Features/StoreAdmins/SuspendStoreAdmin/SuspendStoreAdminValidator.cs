using FluentValidation;

namespace ThriftMedia.Application.Features.StoreAdmins.SuspendStoreAdmin;

public class SuspendStoreAdminValidator : AbstractValidator<SuspendStoreAdminCommand>
{
    public SuspendStoreAdminValidator()
    {
        RuleFor(x => x.AppUserId).GreaterThan(0).WithMessage("A valid user ID is required.");
    }
}
