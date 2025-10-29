using Microsoft.Extensions.Logging;

namespace ThriftMedia.Application.Pipelines;

/// <summary>
/// Orchestrates the execution of media processing steps in a sequential pipeline.
/// </summary>
public class MediaProcessingPipeline
{
    private readonly IEnumerable<IMediaProcessingStep> _steps;
    private readonly ILogger<MediaProcessingPipeline> _logger;

    public MediaProcessingPipeline(
        IEnumerable<IMediaProcessingStep> steps,
        ILogger<MediaProcessingPipeline> logger)
    {
        _steps = steps ?? throw new ArgumentNullException(nameof(steps));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes all registered processing steps in order.
    /// Stops on first failure and returns the failed result.
    /// </summary>
    public async Task<MediaProcessingResult> ExecuteAsync(
        MediaProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting media processing pipeline for MediaId: {MediaId}, StoreId: {StoreId}",
            context.MediaId,
            context.StoreId);

        foreach (var step in _steps)
        {
            _logger.LogInformation(
                "Executing step: {StepName} for MediaId: {MediaId}",
                step.StepName,
                context.MediaId);

            try
            {
                var result = await step.ProcessAsync(context, cancellationToken);

                if (!result.Success)
                {
                    _logger.LogError(
                        "Step {StepName} failed for MediaId: {MediaId}. Error: {ErrorMessage}",
                        step.StepName,
                        context.MediaId,
                        result.ErrorMessage);

                    return result;
                }

                _logger.LogInformation(
                    "Step {StepName} completed successfully for MediaId: {MediaId}",
                    step.StepName,
                    context.MediaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error in step {StepName} for MediaId: {MediaId}",
                    step.StepName,
                    context.MediaId);

                return MediaProcessingResult.Failed(
                    $"Unexpected error in step {step.StepName}: {ex.Message}",
                    ex);
            }
        }

        _logger.LogInformation(
            "Media processing pipeline completed successfully for MediaId: {MediaId}",
            context.MediaId);

        return MediaProcessingResult.Successful();
    }
}
