namespace ThriftMedia.Application.Pipelines;

/// <summary>
/// Represents a single processing step in the media ingestion pipeline.
/// Implement this interface to create extensible processing steps.
/// </summary>
public interface IMediaProcessingStep
{
    /// <summary>
    /// The name of the processing step for logging and identification.
    /// </summary>
    string StepName { get; }

    /// <summary>
    /// Executes the processing step logic.
    /// </summary>
    /// <param name="context">The processing context containing media information and metadata.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>Result indicating success or failure of the step.</returns>
    Task<MediaProcessingResult> ProcessAsync(
        MediaProcessingContext context,
        CancellationToken cancellationToken = default);
}
