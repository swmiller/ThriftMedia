using Microsoft.EntityFrameworkCore;
using ThriftMedia.Application.Queries;
using ThriftMedia.Contracts.Dto;
using ThriftMedia.Infrastructure.Persistence.Models;
using ThriftMedia.Mediator;

namespace ThriftMedia.Api.Features.Stores.GetAllStores
{
    public class GetAllStoresQuery : IRequest<IEnumerable<StoreDto>>;

    public class GetAllStoresQueryHandler : IRequestHandler<GetAllStoresQuery, IEnumerable<StoreDto>>
    {
        private readonly ThriftMediaDbContext _dbContext;

        public GetAllStoresQueryHandler(ThriftMediaDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public async Task<IEnumerable<StoreDto>> Handle(GetAllStoresQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            var allStores = await _dbContext.Stores
                .AsNoTracking()
                .Select( s => new StoreDto(
                    s.Id,
                    s.StoreName,
                    s.PhoneNumber,
                    s.WebsiteUrl,
                    s.IsActive,
                    s.IsSuspended,
                    s.OwnerFirstName,
                    s.OwnerLastName,
                    s.OwerPhoneNumber,
                    s.OwerEmail,
                    s.LicenseNumber,
                    s.LicenseType,
                    s.IssueingAuthority,
                    s.IssueDate,
                    s.ExpirationDate,
                    s.LicenseStatus,
                    s.Address1,
                    s.Address2,
                    s.City,
                    s.Postcode,
                    s.Country,
                    s.AppUserId,
                    s.ProvinceState
                ))
                .ToListAsync(cancellationToken);

            return allStores;
        }
    }
}
