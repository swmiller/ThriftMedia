using System.Data.Common;
using Npgsql;

namespace ThriftMedia.Infrastructure.Data;

/// <summary>
/// Opens connections from the shared <see cref="NpgsqlDataSource"/> (pooling, telemetry and
/// health checks are configured by the host, e.g. Aspire's AddNpgsqlDataSource).
/// </summary>
internal sealed class NpgsqlConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
        => await dataSource.OpenConnectionAsync(cancellationToken);
}
