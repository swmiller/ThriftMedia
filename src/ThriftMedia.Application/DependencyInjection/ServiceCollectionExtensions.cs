using Microsoft.Extensions.DependencyInjection;
using ThriftMedia.Mediator;
using System.Reflection;

namespace ThriftMedia.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register Mediator with handlers from Application assembly
        services.AddMediator(config =>
        {
            config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
        });

        return services;
    }
}
