using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ThriftMedia.Mediator;

namespace ThriftMedia.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the mediator, all request handlers, and all FluentValidation validators
    /// found in the Application assembly.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediator(config => config.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<Abstractions.IAccountNotifier, Common.EmailAccountNotifier>();

        return services;
    }
}
