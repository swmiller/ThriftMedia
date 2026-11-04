using ThriftMedia.Application.Abstractions;

namespace ThriftMedia.Infrastructure.Persistence;

internal sealed class UnitOfWork(DbSession session) : IUnitOfWork
{
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken)
    {
        await session.BeginTransactionAsync(cancellationToken);
        try
        {
            await work(cancellationToken);
            await session.CommitAsync(cancellationToken);
        }
        catch
        {
            await session.RollbackAsync();
            throw;
        }
    }
}
