using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using toobeeh.Louvre.Server.Authentication;
using toobeeh.Louvre.Server.Dto;
using toobeeh.Louvre.Server.Service;

namespace toobeeh.Louvre.Server.Controllers;

[ApiController]
[Route("renders")]
public class RendersController(
    ILogger<RendersController> logger,
    RendersService rendersService,
    GifRenderService gifRenderService,
    TypoCloudService typoCloudService
    ) : ControllerBase
{
    
    [HttpPost, Authorize(Roles = "Moderator,Administrator,Contributor")]
    public async Task<IEnumerable<RenderInfoDto>> FindRenders(FindRendersFilterDto filter)
    {
        logger.LogTrace("FindRenders({Filter})", filter);
        
        return await rendersService.FindRenders(filter);
    }
    
    [HttpPost("add"), Authorize(Roles = "Moderator,Administrator,Contributor")]
    public async Task<Ulid> AddRender(AddRenderDto renderDto)
    {
        logger.LogTrace("AddRender({RenderDto})", renderDto);

        var user = TypoAuthenticationHelper.GetUserFromPrincipal(User);
        var token = TypoAuthenticationHelper.GetAccessTokenFromPrincipal(User);
        
        var cloudImage = await typoCloudService.GetCloudImage(user.Login, token, renderDto.CloudId);
        var commands = await typoCloudService.GetSkdFromCloud(cloudImage);
        var image = await typoCloudService.GetImageFromCloud(cloudImage);
        var gif = await gifRenderService.RenderGif(commands, renderDto.DurationSeconds, renderDto.FramesPerSecond, renderDto.OptimizationLevelPercent);
        
        await System.IO.File.WriteAllBytesAsync("/tmp/" + gif.RenderId + ".gif", gif.GifContent);

        return gif.RenderId;
    }
}