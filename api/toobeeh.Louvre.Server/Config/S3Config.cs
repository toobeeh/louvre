namespace toobeeh.Louvre.Server.Config;

public class S3Config
{
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public required string Endpoint { get; init; }
    public required string ExternalBaseUrl { get; init; }
}