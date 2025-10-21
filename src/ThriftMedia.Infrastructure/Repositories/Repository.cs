using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ThriftMedia.Data.Models; // DbContext and entities

namespace ThriftMedia.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ThriftMediaDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(ThriftMediaDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id) => await Set.FindAsync(id);
    public async Task<IReadOnlyList<T>> GetAllAsync() => await Set.ToListAsync();
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate) => await Set.Where(predicate).ToListAsync();
    public async Task AddAsync(T entity) => await Set.AddAsync(entity);
    public void Update(T entity) => Set.Update(entity);
    public void Remove(T entity) => Set.Remove(entity);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Context.SaveChangesAsync(ct);
}