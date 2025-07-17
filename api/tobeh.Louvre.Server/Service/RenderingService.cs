using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Options;
using tobeh.Louvre.Server.Config;

namespace tobeh.Louvre.Server.Service;

public class RenderingService(ILogger<RenderingService> logger, IOptions<RendererConfig> options) 
{
    public record GifRenderResult(Ulid RenderId, byte[] GifContent, int Duration, int Fps, int Optimization);
    
    public async Task<GifRenderResult> RenderGif(Ulid id, double[][] commands, int? duration = null, int? fps = null, int? optimization = null)
    {
        logger.LogTrace("RenderGif({Id}, {CommandsLength}, {Duration}, {Fps}, {Optimization})", id, commands.Length, duration, fps, optimization);
        
        var config = options.Value;
        var durationValue = duration ?? config.DefaultDuration;
        var fpsValue = fps ?? config.DefaultFps;
        var optimizationValue = optimization ?? config.DefaultOptimization;

        var gifBytes = await RunTypoRenderer(commands, id, durationValue, fpsValue);
        gifBytes = await RunGifOptimization(gifBytes, id, optimizationValue);

        return new GifRenderResult(id, gifBytes, durationValue, fpsValue, optimizationValue);
    }
    
    private async Task<byte[]> RunTypoRenderer(double[][] commands, Ulid renderId, int duration, int fps)
    {
        logger.LogTrace("RunTypoRenderer({Commands}, {RenderId})", commands.Length, renderId);
        
        // save commands as skd (json serialized)
        var skdPath = Path.Combine(options.Value.TempDirectory, $"{renderId}.skd");
        await File.WriteAllTextAsync(skdPath, System.Text.Json.JsonSerializer.Serialize(commands));

        var config = options.Value;
        var gifOutputPath = Path.Combine(config.TempDirectory, $"{renderId}-render.gif");
        var args = $"{config.RendererJsLocation} --commandsSkdPath={skdPath} --gifOutputPath={gifOutputPath} --gifFramerate={fps} --gifDuration={duration * 1000}";
        
        logger.LogDebug("Running command: {Command}", args);
        BufferedCommandResult? result = null;
        try
        {
            result = await Cli
                .Wrap("node")
                .WithArguments(args)
                .ExecuteBufferedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gif optimization failed");
        }
        
        // clean up tmp skd
        File.Delete(skdPath);

        if (result?.IsSuccess is not true)
        {
            throw new InvalidOperationException($"Renderer failed with exit code {result?.ExitCode}: {result?.StandardError}");
        }
        
        // read the output gif file
        if (!File.Exists(gifOutputPath))
        {
            throw new FileNotFoundException("Rendered GIF file not found", gifOutputPath);
        }
        var gifContent = await File.ReadAllBytesAsync(gifOutputPath);
        
        // clean up the output gif file
        File.Delete(gifOutputPath);

        return gifContent;
    }

    private async Task<byte[]> RunGifOptimization(byte[] sourceGif, Ulid renderId, int optimization)
    {
        logger.LogTrace("RunGifOptimization({RenderId})", renderId);
        
        // save gif to temp file
        var sourceGifPath = Path.Combine(options.Value.TempDirectory, $"{renderId}-source.gif");
        await File.WriteAllBytesAsync(sourceGifPath, sourceGif);
        
        var config = options.Value;
        var optimizedGifPath = Path.Combine(config.TempDirectory, $"{renderId}-optimized.gif");
        var args = $"--optimize={optimization} {sourceGifPath} -o {optimizedGifPath}";
        
        logger.LogDebug("Running optimization command: {Command}", args);
        BufferedCommandResult? result = null;
        try
        {
            result = await Cli
                .Wrap(config.GifsicleLocation)
                .WithArguments(args)
                .ExecuteBufferedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gif optimization failed");
        }
        
        // clean up tmp source gif
        File.Delete(sourceGifPath);
        
        if (result?.IsSuccess is not true)
        {
            throw new InvalidOperationException($"Gif optimization failed with exit code {result?.ExitCode}: {result?.StandardError}");
        }
        
        // read the optimized gif file
        if (!File.Exists(optimizedGifPath))
        {
            throw new FileNotFoundException("Optimized GIF file not found", optimizedGifPath);
        }
        var optimizedGifContent = await File.ReadAllBytesAsync(optimizedGifPath);
        
        // clean up the optimized gif file
        File.Delete(optimizedGifPath);
        
        return optimizedGifContent;
    }
}