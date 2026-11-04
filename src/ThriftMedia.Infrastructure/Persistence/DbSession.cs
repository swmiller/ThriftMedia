using System.Data.Common;
using ThriftMedia.Infrastructure.Data;

namespace ThriftMedia.Infrastructure.Persistence;

/// <summary>
/// Scoped holder of the connection (and, inside a unit of work, the transaction) shared by all
/// repositories in one request, so they participate in the same transaction.
/// </summary>
internal sealed class DbSession(IDbConnectionFactory connectionFactory) : IAsyncDisposable
{
    private DbConnection? _connection;

    public DbTransaction? Transaction { get; private set; }

    public bool InTransaction => Transaction is not null;

    public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
        => _connection ??= await connectionFactory.OpenConnectionAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (Transaction is not null)
            throw new InvalidOperationException("A transaction is already in progress");

        var connection = await GetConnectionAsync(cancellationToken);
        Transaction = await connection.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (Transaction is null) return;
        await Transaction.CommitAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        if (Transaction is null) return;
        try { await Transaction.RollbackAsync(CancellationToken.None); }
        finally { await DisposeTransactionAsync(); }
    }

    private async Task DisposeTransactionAsync()
    {
        if (Transaction is null) return;
        await Transaction.DisposeAsync();
        Transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
