using tobeh.Louvre.Server.Service.Data;

namespace tobeh.Louvre.Server.Service;

public class RenderTaskWorkerService (
    ILogger<RenderingService> logger,
    StorageService storageService,
    TypoCloudService typoCloudService,
    RenderingService renderingService,
    RendersService rendersService
    )
{
    public async Task SubmitAndRender(RenderSubmissionData submission)
    {
        logger.LogTrace("SubmitAndRender({Submission})", submission);

        logger.LogInformation("Starting rendering of {RenderId} for user {UserLogin}", submission.Render.Id, submission.User.Login);
        
        var commands = await typoCloudService.GetSkdFromCloud(submission.CloudImage);
        var image = await typoCloudService.GetImageFromCloud(submission.CloudImage);
        var gif = await renderingService.RenderGif(
            submission.Render.Id, 
            commands, 
            submission.RenderSubmissionDetails.RenderParameters?.DurationSeconds, 
            submission.RenderSubmissionDetails.RenderParameters?.FramesPerSecond, 
            submission.RenderSubmissionDetails.RenderParameters?.OptimizationLevelPercent
            );

        await storageService.SaveGif(gif.RenderId, gif.GifContent);
        await storageService.SaveThumbnail(gif.RenderId, image);
        await rendersService.SetRenderCompleted(gif);
        
        logger.LogInformation("Rendering of {RenderId} completed for user {UserLogin}", submission.Render.Id, submission.User.Login);
    }
    
    public async Task Rerender(RerenderRequestData request)
    {
        logger.LogTrace("Rerender({Request})", request);

        logger.LogInformation("Starting rerendering of {RenderId}", request.Render.Id);
        
        await storageService.TryRemoveGif(request.Render.Id);
        
        var commands = await typoCloudService.GetSkdFromCloud(request.CloudImage);
        var gif = await renderingService.RenderGif(
            request.Render.Id, 
            commands, 
            request.Parameters?.DurationSeconds, 
            request.Parameters?.FramesPerSecond, 
            request.Parameters?.OptimizationLevelPercent
            );

        await storageService.SaveGif(gif.RenderId, gif.GifContent);
        await rendersService.SetRenderCompleted(gif);
        
        logger.LogInformation("Rerendering of {RenderId} completed", request.Render.Id);
    }
}