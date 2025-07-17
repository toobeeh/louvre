namespace tobeh.Louvre.Server.Service;

public class RenderSubmissionWorkerService (
    ILogger<RenderingService> logger,
    StorageService storageService,
    TypoCloudService typoCloudService,
    RenderingService renderingService,
    RendersService rendersService
    )
{
    public async Task SubmitAndRender(RenderSubmissionDispatcherService.RenderSubmission submission)
    {
        logger.LogTrace("SubmitAndRender({Submission})", submission);

        logger.LogInformation("Starting rendering of {RenderId} for user {UserLogin}", submission.Render.Id, submission.User.Login);
        
        var commands = await typoCloudService.GetSkdFromCloud(submission.CloudImage);
        var image = await typoCloudService.GetImageFromCloud(submission.CloudImage);
        var gif = await renderingService.RenderGif(
            submission.Render.Id, 
            commands, 
            submission.RenderSubmissionDetails.DurationSeconds, 
            submission.RenderSubmissionDetails.FramesPerSecond, 
            submission.RenderSubmissionDetails.OptimizationLevelPercent
            );

        await storageService.SaveGif(gif.RenderId, gif.GifContent);
        await storageService.SaveThumbnail(gif.RenderId, image);
        await rendersService.SetRenderCompleted(gif);
        
        logger.LogInformation("Rendering of {RenderId} completed for user {UserLogin}", submission.Render.Id, submission.User.Login);
    }
}