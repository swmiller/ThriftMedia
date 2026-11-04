using FluentValidation;

namespace ThriftMedia.Application.Features.Stores.RenameStore;

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
