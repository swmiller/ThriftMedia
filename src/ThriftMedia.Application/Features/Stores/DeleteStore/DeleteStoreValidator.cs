using FluentValidation;

namespace ThriftMedia.Application.Features.Stores.DeleteStore;

public class DeleteStoreValidator : AbstractValidator<DeleteStoreCommand>
{
    public DeleteStoreValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("A valid store ID is required.");
    }
}
