namespace tobeh.Louvre.Server.Config;

public class RendererConfig
{
    public required string TempDirectory { get; init; }
    public required string RendererJsLocation { get; init; }
    public required string GifsicleLocation { get; init; }
    public required int DefaultFps { get; init; }
    public required int DefaultDuration { get; init; }
    public required int DefaultOptimization { get; init; }
    public required int SubmissionConcurrency { get; init; }
    public string? WatermarkLocation { get; init; }
}