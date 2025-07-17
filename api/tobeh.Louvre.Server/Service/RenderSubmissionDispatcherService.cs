using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using tobeh.Louvre.Server.Config;
using tobeh.Louvre.Server.Dto;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Service;

public class RenderSubmissionDispatcherService(
    ILogger<RenderSubmissionDispatcherService> logger, 
    IServiceScopeFactory scopeFactory,
    IOptions<RendererConfig> options)
{
    public record RenderSubmission(
        CloudImageDto CloudImage,
        RenderInfoDto Render,
        RenderSubmissionDto RenderSubmissionDetails,
        AuthorizedUserDto User
    );
    
    private readonly SemaphoreSlim _submissionSemaphore = new(options.Value.SubmissionConcurrency);
    
    public async Task EnqueueSubmission(RenderSubmission submission)
    {
        logger.LogTrace("EnqueueSubmission({Submission})", submission);
        
        logger.LogInformation("Submitted {id} with currently {concurrency} free queue slots", 
            submission.Render.Id, _submissionSemaphore.CurrentCount);
        
        await _submissionSemaphore.WaitAsync();
        try
        {
            var scope = scopeFactory.CreateScope();
            var gifRenderWorkerService = scope.ServiceProvider.GetRequiredService<RenderSubmissionWorkerService>();
            await gifRenderWorkerService.SubmitAndRender(submission);
        }
        catch (Exception e)
        {
            logger.LogError("Error processing submission: {Error}", e.Message);
        }
        finally
        {
            _submissionSemaphore.Release();
        }
        
    }
}