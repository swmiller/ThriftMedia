using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThriftMedia.Application.Repositories;
using ThriftMedia.Domain.Services;
using ThriftMedia.Infrastructure.Persistence.Models;
using ThriftMedia.Infrastructure.Repositories;
using ThriftMedia.Infrastructure.Services;

namespace ThriftMedia.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ThriftMediaDb")
            ?? throw new InvalidOperationException("Connection string 'ThriftMediaDb' not found.");

        services.AddDbContext<ThriftMediaDbContext>(options => options.UseNpgsql(connectionString));

        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register specific repositories
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();

        // Register domain services
        services.AddScoped<IMediaModerationService, MediaModerationService>();

        return services;
    }
}