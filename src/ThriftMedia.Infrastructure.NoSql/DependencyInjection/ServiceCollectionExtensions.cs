using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ThriftMedia.Infrastructure.NoSql.Repositories;

namespace ThriftMedia.Infrastructure.NoSql.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNoSqlInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var noSqlConfig = configuration.GetSection("NoSql");
        var connectionString = noSqlConfig["ConnectionString"];
        var databaseName = noSqlConfig["Database"];

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddScoped<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
        services.AddScoped<INoSqlMediaRepository, NoSqlMediaRepository>();
        // Register other NoSQL repositories as needed
        return services;
    }
}
