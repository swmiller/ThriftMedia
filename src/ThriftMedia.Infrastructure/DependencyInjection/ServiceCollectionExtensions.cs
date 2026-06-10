using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ThriftMedia.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: determine if this file is still necessary.
        return services;
    }
}