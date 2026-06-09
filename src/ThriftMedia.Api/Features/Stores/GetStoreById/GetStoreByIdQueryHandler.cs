using Microsoft.EntityFrameworkCore;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Infrastructure.Persistence.Models;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.GetStoreById
{
    public record GetStoreByIdQuery(int Id) : IRequest<StoreDto?>;

    public class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, StoreDto?>
    {
        private readonly ThriftMediaDbContext _dbContext;

        public GetStoreByIdQueryHandler(ThriftMediaDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<StoreDto?> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
        {
            var store = await _dbContext.Stores
                .AsNoTracking()
                .Where(s => s.Id == request.Id)
                .Select(s => new StoreDto(
                    s.Id,
                    s.StoreName,
                    s.PhoneNumber,
                    s.WebsiteUrl,
                    s.IsActive,
                    s.IsSuspended,
                    s.OwnerFirstName,
                    s.OwnerLastName,
                    s.OwnerPhoneNumber,
                    s.OwnerEmail,
                    s.LicenseNumber,
                    s.LicenseType,
                    s.IssueingAuthority,
                    s.IssueDate,
                    s.ExpirationDate,
                    s.LicenseStatus,
                    s.Address1,
                    s.Address2,
                    s.City,
                    s.PostalCode,
                    s.Country,
                    s.AppUserId,
                    s.ProvinceState
                ))
                .FirstOrDefaultAsync(cancellationToken);

            return store;
        }
    }
}
