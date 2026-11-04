// using ThriftMedia.Infrastructure.Persistence.Models;

namespace ThriftMedia.Application.Features.Stores.DeleteStore;

// TODO: EF Core removed - reimplement with Dapper in Application/Infrastructure
//     public class DeleteStoreCommandHandler : IRequestHandler<DeleteStoreCommand, bool>
//     {
//         private readonly ThriftMediaDbContext _dbContext;
//         private readonly IValidator<DeleteStoreCommand> _validator;
// 
//         public DeleteStoreCommandHandler(ThriftMediaDbContext dbContext, IValidator<DeleteStoreCommand> validator)
//         {
//             _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
//             _validator = validator ?? throw new ArgumentNullException(nameof(validator));
//         }
// 
//         public async Task<bool> Handle(DeleteStoreCommand command, CancellationToken cancellationToken)
//         {
//             await _validator.ValidateAndThrowAsync(command, cancellationToken);
// 
//             var store = await _dbContext.Stores
//                 .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
// 
//             if (store is null)
//                 return false;
// 
//             _dbContext.Stores.Remove(store);
//             await _dbContext.SaveChangesAsync(cancellationToken);
// 
//             return true;
//         }
//     }
