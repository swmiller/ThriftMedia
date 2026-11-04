// using DomainStore = ThriftMedia.Domain.Entities.Store;
// using DomainAddress = ThriftMedia.Domain.ValueObjects.Address;
// using PersistenceStore = ThriftMedia.Infrastructure.Persistence.Models.Store;

namespace ThriftMedia.Application.Features.Stores.CreateStore;

// TODO: EF Core removed - reimplement with Dapper in Application/Infrastructure
//     public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, int>
//     {
//         private readonly ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext _dbContext;
//         private readonly IValidator<CreateStoreCommand> _validator;
//         private readonly TimeProvider _timeProvider;
// 
//         public CreateStoreCommandHandler(ThriftMedia.Infrastructure.Persistence.Models.ThriftMediaDbContext dbContext, IValidator<CreateStoreCommand> validator, TimeProvider timeProvider)
//         {
//             _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
//             _validator = validator ?? throw new ArgumentNullException(nameof(validator));
//             _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
//         }
// 
//         public async Task<int> Handle(CreateStoreCommand command, CancellationToken cancellationToken)
//         {
//             await _validator.ValidateAndThrowAsync(command, cancellationToken);
// 
//             var req = command.request;
// 
//             var address = DomainAddress.Create(
//                 req.Address1,
//                 req.Address2,
//                 req.City,
//                 req.ProvinceState,
//                 req.PostalCode,
//                 req.Country ?? string.Empty);
// 
//             var domainStore = DomainStore.Create(
//                 req.StoreName,
//                 req.LicenseNumber,
//                 req.LicenseType,
//                 req.IssueingAuthority,
//                 req.IssueDate,
//                 req.ExpirationDate,
//                 req.LicenseStatus,
//                 address,
//                 req.AppUserId,
//                 "system", // TODO: replace with authenticated user
//                 _timeProvider.GetUtcNow().UtcDateTime,
//                 phoneNumber: req.PhoneNumber,
//                 websiteUrl: req.WebsiteUrl,
//                 ownerFirstName: req.OwnerFirstName,
//                 ownerLastName: req.OwnerLastName,
//                 ownerPhoneNumber: req.OwnerPhoneNumber,
//                 ownerEmail: req.OwnerEmail);
// 
//             var persistenceStore = new PersistenceStore
//             {
//                 StoreName = domainStore.StoreName,
//                 PhoneNumber = domainStore.PhoneNumber,
//                 WebsiteUrl = domainStore.WebsiteUrl,
//                 IsActive = domainStore.IsActive,
//                 IsSuspended = domainStore.IsSuspended,
//                 OwnerFirstName = domainStore.OwnerFirstName,
//                 OwnerLastName = domainStore.OwnerLastName,
//                 OwnerPhoneNumber = domainStore.OwnerPhoneNumber,
//                 OwnerEmail = domainStore.OwnerEmail,
//                 LicenseNumber = domainStore.LicenseNumber,
//                 LicenseType = domainStore.LicenseType,
//                 IssueingAuthority = domainStore.IssueingAuthority,
//                 IssueDate = domainStore.IssueDate,
//                 ExpirationDate = domainStore.ExpirationDate,
//                 LicenseStatus = domainStore.LicenseStatus,
//                 Address1 = domainStore.Address.Line1,
//                 Address2 = domainStore.Address.Line2 ?? string.Empty,
//                 City = domainStore.Address.City,
//                 PostalCode = domainStore.Address.PostalCode,
//                 Country = domainStore.Address.Country,
//                 ProvinceState = domainStore.Address.ProvinceState,
//                 AppUserId = domainStore.AppUserId,
//                 CreatedBy = domainStore.CreatedBy,
//                 CreatedAt = domainStore.CreatedAt,
//             };
// 
//             _dbContext.Stores.Add(persistenceStore);
//             await _dbContext.SaveChangesAsync(cancellationToken);
// 
//             return persistenceStore.Id;
//         }
//     }
