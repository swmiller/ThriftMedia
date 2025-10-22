using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThriftMedia.Data.Models; // DbContext resides in Data.Models
using ThriftMedia.Domain.Services;
using ThriftMedia.Infrastructure.Repositories;
using ThriftMedia.Infrastructure.Services;

namespace ThriftMedia.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ThriftMediaDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IMediaModerationService, MediaModerationService>();

        return services;
    }
}