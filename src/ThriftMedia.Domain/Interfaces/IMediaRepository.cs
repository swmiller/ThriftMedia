using System;
using System.Threading;
using System.Threading.Tasks;
using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Domain.Interfaces
{
    public interface IMediaRepository
    {
        Task<Media?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        // Add other CRUD methods as needed
    }
}
