using Microsoft.Extensions.DependencyInjection;
using ThriftMedia.Mediator;
using System.Reflection;
using ThriftMedia.Application.Pipelines;
using ThriftMedia.Application.Pipelines.Steps;

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

        // Register validators from Application assembly (if using FluentValidation)
        // TODO: AddValidatorsFromAssembly not found. Fix later when solution cleanup is done.
        //services.AddValidatorsFromAssembly(Assembly.GetCallingAssembly());

        // Register media processing pipeline and steps
        services.AddTransient<MediaProcessingPipeline>();
        services.AddTransient<IMediaProcessingStep, OcrProcessingStep>();
        services.AddTransient<IMediaProcessingStep, MediaClassificationStep>();
        services.AddTransient<IMediaProcessingStep, ContentModerationStep>();
        services.AddTransient<IMediaProcessingStep, CatalogListingStep>();

        return services;
    }
}
