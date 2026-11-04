using System.Data.Common;

namespace ThriftMedia.Infrastructure.Data;

/// <summary>
/// Creates open database connections for Dapper repositories.
/// Callers own the returned connection and must dispose it.
/// </summary>
public interface IDbConnectionFactory
{
    Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
