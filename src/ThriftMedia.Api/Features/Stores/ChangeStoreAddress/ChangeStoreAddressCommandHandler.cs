using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Infrastructure.Persistence.Models;
using ThriftMedia.Mediator;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ThriftMedia.Api.Features.Stores.ChangeStoreAddress
{
    public record ChangeStoreAddressCommand(int Id, ChangeStoreAddressRequest Request) : IRequest<bool>;

    public class ChangeStoreAddressCommandValidator : AbstractValidator<ChangeStoreAddressCommand>
    {
        public ChangeStoreAddressCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("A valid store ID is required.");

            RuleFor(x => x.Request.Address1)
                .NotEmpty()
                .WithMessage("Address line 1 is required.")
                .MaximumLength(200)
                .WithMessage("Address line 1 must not exceed 200 characters.");

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
        }
    }

    public class ChangeStoreAddressCommandHandler : IRequestHandler<ChangeStoreAddressCommand, bool>
    {
        private readonly ThriftMediaDbContext _dbContext;
        private readonly IValidator<ChangeStoreAddressCommand> _validator;

        public ChangeStoreAddressCommandHandler(ThriftMediaDbContext dbContext, IValidator<ChangeStoreAddressCommand> validator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<bool> Handle(ChangeStoreAddressCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var store = await _dbContext.Stores
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (store is null)
                return false;

            store.Address1 = command.Request.Address1;
            store.Address2 = command.Request.Address2 ?? string.Empty;
            store.City = command.Request.City;
            store.PostalCode = command.Request.PostalCode;
            store.ProvinceState = command.Request.ProvinceState;
            store.Country = command.Request.Country;
            store.UpdatedBy = "system"; // TODO: replace with authenticated user
            store.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
