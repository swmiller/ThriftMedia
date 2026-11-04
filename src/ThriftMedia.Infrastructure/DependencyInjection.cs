using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Infrastructure.Data;
using ThriftMedia.Infrastructure.Email;
using ThriftMedia.Infrastructure.Persistence;

namespace ThriftMedia.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure services. The host must register an <c>NpgsqlDataSource</c>
    /// (the Api does this with Aspire's <c>AddNpgsqlDataSource("ThriftMediaDb")</c>).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.TryAddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

        // One session (connection + optional transaction) per request, shared by repositories.
        services.TryAddScoped<DbSession>();
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();
        services.TryAddScoped<IAppUserRepository, AppUserRepository>();
        services.TryAddScoped<IStoreApplicationRepository, StoreApplicationRepository>();
        services.TryAddScoped<IStoreRepository, StoreRepository>();

        services.TryAddSingleton<IEmailSender, LoggingEmailSender>();
        return services;
    }
}
