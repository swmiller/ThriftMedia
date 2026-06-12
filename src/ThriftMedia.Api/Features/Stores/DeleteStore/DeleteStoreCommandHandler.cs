using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ThriftMedia.Infrastructure.Persistence.Models;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.DeleteStore
{
    public record DeleteStoreCommand(int Id) : IRequest<bool>;

    public class DeleteStoreValidator : AbstractValidator<DeleteStoreCommand>
    {
        public DeleteStoreValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("A valid store ID is required.");
        }
    }

    public class DeleteStoreCommandHandler : IRequestHandler<DeleteStoreCommand, bool>
    {
        private readonly ThriftMediaDbContext _dbContext;
        private readonly IValidator<DeleteStoreCommand> _validator;

        public DeleteStoreCommandHandler(ThriftMediaDbContext dbContext, IValidator<DeleteStoreCommand> validator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<bool> Handle(DeleteStoreCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var store = await _dbContext.Stores
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);

            if (store is null)
                return false;

            _dbContext.Stores.Remove(store);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
