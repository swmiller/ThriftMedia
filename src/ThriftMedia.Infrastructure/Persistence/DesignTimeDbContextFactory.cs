using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ThriftMedia.Data.Models;

namespace ThriftMedia.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ThriftMediaDbContext>
{
    public ThriftMediaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ThriftMediaDbContext>();
        var connectionString = "Host=localhost;Database=ThriftMediaDb;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(connectionString);
        return new ThriftMediaDbContext(optionsBuilder.Options);
    }
}