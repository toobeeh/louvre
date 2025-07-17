using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tobeh.Louvre.Server.Authentication;
using tobeh.Louvre.Server.Dto;
using tobeh.Louvre.Server.Service;

namespace tobeh.Louvre.Server.Controllers;

[ApiController]
[Route("renders")]
public class RendersController(
    ILogger<RendersController> logger,
    RendersService rendersService,
    TypoCloudService typoCloudService,
    RenderSubmissionDispatcherService renderSubmissionDispatcherService
    ) : ControllerBase
{
    
    [HttpPost, Authorize(Roles = "Moderator,Administrator,Contributor")]
    public async Task<IEnumerable<RenderInfoDto>> FindRenders(FindRendersFilterDto filter)
    {
        logger.LogTrace("FindRenders({Filter})", filter);
        
        return await rendersService.FindRenders(filter);
    }
    
    [HttpPost("submit"), Authorize(Roles = "Moderator,Administrator,Contributor")]
    public async Task<RenderInfoDto> Submit(RenderSubmissionDto renderSubmissionDto)
    {
        logger.LogTrace("AddRender({RenderDto})", renderSubmissionDto);

        var user = TypoAuthenticationHelper.GetUserFromPrincipal(User);
        var token = TypoAuthenticationHelper.GetAccessTokenFromPrincipal(User);
        
        // fetch image details from typo cloud
        var cloudImage = await typoCloudService.GetCloudImage(user.Login, token, renderSubmissionDto.CloudId);

        // submit as rendering
        var render = await rendersService.AddRenderRequest(cloudImage, user.Login);
        
        // run render in background, will set to render finished when done
        renderSubmissionDispatcherService
            .EnqueueSubmission(new RenderSubmissionDispatcherService.RenderSubmission(
                cloudImage,
                render,
                renderSubmissionDto,
                user
            ));

        return render;
    }
}