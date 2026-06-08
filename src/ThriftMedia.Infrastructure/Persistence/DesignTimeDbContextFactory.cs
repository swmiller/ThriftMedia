using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ThriftMedia.Infrastructure.Persistence.Models;

namespace ThriftMedia.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ThriftMediaDbContext>
{
    public ThriftMediaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ThriftMediaDbContext>();
        var connectionString = "Server=localhost,1433;Database=ThriftMediaDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True";
        optionsBuilder.UseSqlServer(connectionString);
        return new ThriftMediaDbContext(optionsBuilder.Options);
    }
}