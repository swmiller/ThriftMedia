using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ThriftMedia.Mediator;

/// <summary>
/// Extension methods for setting up mediator services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds mediator services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configuration">Optional configuration action</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        Action<MediatorConfiguration>? configuration = null)
    {
        var config = new MediatorConfiguration(services);
        configuration?.Invoke(config);

        services.TryAddTransient<IMediator, Mediator>();
        services.TryAddTransient<ISender>(sp => sp.GetRequiredService<IMediator>());
        services.TryAddTransient<IPublisher>(sp => sp.GetRequiredService<IMediator>());

        return services;
    }
}

/// <summary>
/// Configuration for mediator registration.
/// </summary>
public class MediatorConfiguration
{
    private readonly IServiceCollection _services;

    public MediatorConfiguration(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// Register all request handlers and notification handlers from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for handlers</param>
    /// <returns>The configuration instance for chaining</returns>
    public MediatorConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            RegisterServicesFromAssembly(assembly);
        }

        return this;
    }

    /// <summary>
    /// Register all request handlers and notification handlers from the specified assembly.
    /// </summary>
    /// <param name="assembly">Assembly to scan for handlers</param>
    /// <returns>The configuration instance for chaining</returns>
    public MediatorConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
            .ToList();

        // Register request handlers
        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();

                // Register IRequestHandler<TRequest, TResponse>
                if (genericTypeDefinition == typeof(IRequestHandler<,>))
                {
                    _services.AddTransient(interfaceType, type);
                }
                // Register INotificationHandler<TNotification>
                else if (genericTypeDefinition == typeof(INotificationHandler<>))
                {
                    _services.AddTransient(interfaceType, type);
                }
            }
        }

        return this;
    }

    /// <summary>
    /// Register all request handlers and notification handlers from the assembly containing the specified type.
    /// </summary>
    /// <param name="type">Type whose assembly to scan for handlers</param>
    /// <returns>The configuration instance for chaining</returns>
    public MediatorConfiguration RegisterServicesFromAssemblyContaining(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return RegisterServicesFromAssembly(type.Assembly);
    }

    /// <summary>
    /// Register all request handlers and notification handlers from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">Type whose assembly to scan for handlers</typeparam>
    /// <returns>The configuration instance for chaining</returns>
    public MediatorConfiguration RegisterServicesFromAssemblyContaining<T>()
    {
        return RegisterServicesFromAssemblyContaining(typeof(T));
    }
}
