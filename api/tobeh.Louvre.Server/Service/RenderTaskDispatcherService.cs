using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using tobeh.Louvre.Server.Config;
using tobeh.Louvre.Server.Service.Data;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Service;

public class RenderTaskDispatcherService(
    ILogger<RenderTaskDispatcherService> logger, 
    IServiceScopeFactory scopeFactory,
    IOptions<RendererConfig> options)
{
    private readonly SemaphoreSlim _submissionSemaphore = new(options.Value.SubmissionConcurrency);
    
    public async Task EnqueueSubmission(RenderSubmissionData submission)
    {
        logger.LogTrace("EnqueueSubmission({Submission})", submission);
        
        logger.LogInformation("Submitted {id} with currently {concurrency} free queue slots", 
            submission.Render.Id, _submissionSemaphore.CurrentCount);
        
        await _submissionSemaphore.WaitAsync();
        try
        {
            var scope = scopeFactory.CreateScope();
            var gifRenderWorkerService = scope.ServiceProvider.GetRequiredService<RenderTaskWorkerService>();
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
    
    public async Task EnqueueRerender(RerenderRequestData request)
    {
        logger.LogTrace("EnqueueRerender({Request})", request);
        
        logger.LogInformation("Rerendering {id} with currently {concurrency} free queue slots", 
            request.Render.Id, _submissionSemaphore.CurrentCount);
        
        await _submissionSemaphore.WaitAsync();
        try
        {
            var scope = scopeFactory.CreateScope();
            var gifRenderWorkerService = scope.ServiceProvider.GetRequiredService<RenderTaskWorkerService>();
            await gifRenderWorkerService.Rerender(request);
        }
        catch (Exception e)
        {
            logger.LogError("Error processing rerender request: {Error}", e.Message);
        }
        finally
        {
            _submissionSemaphore.Release();
        }
    }
}