namespace ThriftMedia.Application.Abstractions;

public interface IUnitOfWork
{
    /// <summary>
    /// Runs <paramref name="work"/> in one database transaction. Repository calls made inside share
    /// that transaction; it commits if the delegate completes and rolls back if it throws.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken);
}
