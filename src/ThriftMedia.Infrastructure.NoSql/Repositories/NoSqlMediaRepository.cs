using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Interfaces;
using MongoDB.Driver;

namespace ThriftMedia.Infrastructure.NoSql.Repositories
{
    public class NoSqlMediaRepository : INoSqlMediaRepository
    {
        private readonly IMongoCollection<Media> _mediaCollection;

        public NoSqlMediaRepository(IMongoDatabase database)
        {
            _mediaCollection = database.GetCollection<Media>("Media");
        }

        // Implement IMediaRepository methods using MongoDB
        // Example:
        public async Task<Media?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _mediaCollection.Find(m => m.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        // Add other methods as needed
    }
}
