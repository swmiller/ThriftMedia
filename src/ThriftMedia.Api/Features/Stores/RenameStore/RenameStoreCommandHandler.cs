using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ThriftMedia.Contracts.Requests;
using ThriftMedia.Infrastructure.Persistence.Models;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.RenameStore
{
    public record RenameStoreCommand(int Id, RenameStoreRequest Request) : IRequest<bool>;

    public class  RenameStoreValidator : AbstractValidator<RenameStoreCommand>
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

    public class RenameStoreCommandHandler : IRequestHandler<RenameStoreCommand, bool>
    {
        private readonly ThriftMediaDbContext _dbContext;
        private readonly IValidator<RenameStoreCommand> _validator;

        public RenameStoreCommandHandler(ThriftMediaDbContext dbContext, IValidator<RenameStoreCommand> validator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }
        public async Task<bool> Handle(RenameStoreCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var store = await _dbContext.Stores
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (store is null)
                return false;

            store.StoreName = command.Request.NewStoreName;
            store.UpdatedBy = "system"; // TODO: replace with authenticated user
            store.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
