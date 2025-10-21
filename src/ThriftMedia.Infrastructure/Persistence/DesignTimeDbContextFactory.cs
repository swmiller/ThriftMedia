using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ThriftMedia.Data.Models;

namespace ThriftMedia.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ThriftMediaDbContext>
{
    public ThriftMediaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ThriftMediaDbContext>();
        var connectionString = "Server=steve-miller;Database=ThriftMediaDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";
        optionsBuilder.UseSqlServer(connectionString);
        return new ThriftMediaDbContext(optionsBuilder.Options);
    }
}