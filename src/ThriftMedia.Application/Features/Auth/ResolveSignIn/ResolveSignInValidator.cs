using FluentValidation;

namespace ThriftMedia.Application.Features.Auth.ResolveSignIn;

public class ResolveSignInValidator : AbstractValidator<ResolveSignInCommand>
{
    public ResolveSignInValidator()
    {
        RuleFor(x => x.Request.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .MaximumLength(200).WithMessage("Provider must not exceed 200 characters.");

        RuleFor(x => x.Request.ProviderSub)
            .NotEmpty().WithMessage("Provider subject is required.")
            .MaximumLength(200).WithMessage("Provider subject must not exceed 200 characters.");

        RuleFor(x => x.Request.Email)
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters.");
    }
}
