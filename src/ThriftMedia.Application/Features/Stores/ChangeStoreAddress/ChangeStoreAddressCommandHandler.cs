// using DomainStore = ThriftMedia.Domain.Entities.Store;
// using DomainAddress = ThriftMedia.Domain.ValueObjects.Address;
// using PersistenceStore = ThriftMedia.Infrastructure.Persistence.Models.Store;

namespace ThriftMedia.Application.Features.Stores.ChangeStoreAddress;

// TODO: EF Core removed - reimplement with Dapper in Application/Infrastructure
//     public class ChangeStoreAddressCommandHandler : IRequestHandler<ChangeStoreAddressCommand, bool>
//     {
//         private const string SystemUser = "system";
// 
//         private readonly ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext _dbContext;
//         private readonly IValidator<ChangeStoreAddressCommand> _validator;
//         private readonly TimeProvider _timeProvider;
// 
//         public ChangeStoreAddressCommandHandler(ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext dbContext, IValidator<ChangeStoreAddressCommand> validator, TimeProvider timeProvider)
//         {
//             _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
//             _validator = validator ?? throw new ArgumentNullException(nameof(validator));
//             _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
//         }
// 
//         public async Task<bool> Handle(ChangeStoreAddressCommand command, CancellationToken cancellationToken)
//         {
//             await _validator.ValidateAndThrowAsync(command, cancellationToken);
// 
//             var persistenceStore = await _dbContext.Stores
//                 .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken);
// 
//             if (persistenceStore is null)
//                 return false;
// 
//             var domainStore = ToDomain(persistenceStore);
// 
//             var newAddress = DomainAddress.Create(
//                 command.Request.Address1,
//                 command.Request.Address2,
//                 command.Request.City,
//                 command.Request.ProvinceState,
//                 command.Request.PostalCode,
//                 command.Request.Country ?? string.Empty);
// 
//             domainStore.ChangeAddress(newAddress, SystemUser, _timeProvider.GetUtcNow().UtcDateTime);
// 
//             ApplyDomainAddressChanges(persistenceStore, domainStore);
// 
//             await _dbContext.SaveChangesAsync(cancellationToken);
// 
//             return true;
//         }
// 
//         private static DomainStore ToDomain(PersistenceStore store)
//         {
//             var address = DomainAddress.Create(
//                 store.Address1, store.Address2, store.City,
//                 store.ProvinceState, store.PostalCode, store.Country ?? string.Empty);
// 
//             return DomainStore.Rehydrate(
//                 store.Id, store.StoreName, store.PhoneNumber, store.WebsiteUrl,
//                 store.IsActive, store.IsSuspended,
//                 store.OwnerFirstName, store.OwnerLastName, store.OwnerPhoneNumber, store.OwnerEmail,
//                 store.LicenseNumber, store.LicenseType, store.IssueingAuthority,
//                 store.IssueDate, store.ExpirationDate, store.LicenseStatus,
//                 address,
//                 store.CreatedBy, store.CreatedAt,
//                 store.UpdatedBy, store.UpdatedAt,
//                 store.AppUserId);
//         }
// 
//         private static void ApplyDomainAddressChanges(PersistenceStore persistenceStore, DomainStore domainStore)
//         {
//             persistenceStore.Address1 = domainStore.Address.Line1;
//             persistenceStore.Address2 = domainStore.Address.Line2 ?? string.Empty;
//             persistenceStore.City = domainStore.Address.City;
//             persistenceStore.PostalCode = domainStore.Address.PostalCode;
//             persistenceStore.ProvinceState = domainStore.Address.ProvinceState;
//             persistenceStore.Country = domainStore.Address.Country;
//             persistenceStore.UpdatedBy = domainStore.UpdatedBy;
//             persistenceStore.UpdatedAt = domainStore.UpdatedAt;
//         }
//     }
