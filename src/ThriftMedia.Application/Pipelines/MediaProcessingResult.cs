namespace ThriftMedia.Application.Pipelines;

/// <summary>
/// Represents the result of a media processing step or pipeline execution.
/// </summary>
public class MediaProcessingResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }

    public static MediaProcessingResult Successful() => new() { Success = true };

    public static MediaProcessingResult Failed(string errorMessage, Exception? exception = null) =>
        new() { Success = false, ErrorMessage = errorMessage, Exception = exception };
}
