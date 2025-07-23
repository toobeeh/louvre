using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tobeh.Louvre.Server.Controllers.Dto;
using tobeh.Louvre.Server.Service;
using tobeh.Louvre.Server.Service.Data;

namespace tobeh.Louvre.Server.Controllers;

[ApiController]
[Route("renders")]
public class RendersController(
    ILogger<RendersController> logger,
    IMapper mapper,
    RendersService rendersService,
    TypoCloudService typoCloudService,
    RenderTaskDispatcherService renderTaskDispatcherService,
    StorageService storageService,
    UserRequestContext userRequestContext
    ) : ControllerBase
{
    
    [HttpPost, Authorize(Policy = "Role:Contributor")]
    public async Task<IEnumerable<RenderPreviewDto>> FindRenders(FindRendersFilterDto filter)
    {
        logger.LogTrace("FindRenders({Filter})", filter);
        
        return mapper.Map<IEnumerable<RenderPreviewDto>>(await rendersService.FindRenders(filter));
    }
    
    [HttpPost("submit"), Authorize(Policy = "Role:Contributor")]
    public async Task<RenderPreviewDto> Submit(RenderSubmissionDto renderSubmissionDto)
    {
        logger.LogTrace("AddRender({RenderDto})", renderSubmissionDto);

        var user = await userRequestContext.GetUserAsync();
        var token = userRequestContext.GetJwt();
        
        // fetch image details from typo cloud
        var cloudImage = await typoCloudService.GetCloudImage(user.TypoId, token, renderSubmissionDto.CloudId);

        // submit as rendering
        var render = await rendersService.AddRenderRequest(cloudImage, user.TypoId);
        
        // run render in background, will set to render finished when done
        renderTaskDispatcherService.EnqueueSubmission(new RenderSubmissionData(
                cloudImage,
                render,
                renderSubmissionDto,
                user
            ));

        return mapper.Map<RenderPreviewDto>(render);
    }

    [HttpGet("{id}"), Authorize(Policy = "Role:Contributor")]
    public async Task<RenderInfoDto> GetRender(Ulid id)
    {
        logger.LogTrace("GetRender({Id})", id);
        
        var render = await rendersService.GetRenderById(id);

        return mapper.Map<RenderInfoDto>(render);
    }

    [HttpDelete("{id}"), Authorize(Policy = "Role:Moderator")]
    public async Task<IActionResult> RemoveRender(Ulid id)
    {
        logger.LogTrace("RemoveRender({Id})", id);
        
        await rendersService.RemoveRender(id);
        await storageService.TryRemoveGif(id);
        await storageService.TryRemoveThumbnail(id);

        return NoContent();
    }
    
    [HttpPatch("{id}/propose/drawer"), Authorize(Policy = "Role:Contributor")]
    public async Task<RenderInfoDto> ProposeRenderDrawer(Ulid id, ProposeRenderDrawerDto proposeRenderDrawerDto)
    {
        logger.LogTrace("ProposeRenderDrawer({Id})", id);
        
        var render = await rendersService.ProposeRenderDetails(id, proposeRenderDrawerDto.DrawerTypoId, null);
        return mapper.Map<RenderInfoDto>(render);
    }
    
    [HttpPatch("{id}/propose/title"), Authorize(Policy = "Role:Contributor")]
    public async Task<RenderInfoDto> ProposeRenderTitle(Ulid id, ProposeRenderTitleDto proposeRenderTitleDto)
    {
        logger.LogTrace("ProposeRenderTitle({Id})", id);
        
        var render = await rendersService.ProposeRenderDetails(id, null, proposeRenderTitleDto.Title);
        return mapper.Map<RenderInfoDto>(render);
    }

    [HttpPatch("{id}/approve"), Authorize(Policy = "Role:Moderator")]
    public async Task<RenderInfoDto> ApproveRender(Ulid id)
    {
        logger.LogTrace("ApproveRender({Id})", id);
        
        var render = await rendersService.ApproveRender(id);
        return mapper.Map<RenderInfoDto>(render);
    }

    [HttpPatch("{id}/unapprove"), Authorize(Policy = "Role:Moderator")]
    public async Task<RenderInfoDto> UnapproveRender(Ulid id)
    {
        logger.LogTrace("UnapproveRender({Id})", id);
        
        var render = await rendersService.UnapproveRender(id);
        return mapper.Map<RenderInfoDto>(render);
    }

    [HttpPatch("{id}/rerender"), Authorize(Policy = "Role:Contributor")]
    public async Task<RenderInfoDto> RerenderRender(Ulid id, RenderParametersDto renderParametersDto)
    {
        logger.LogTrace("RerenderRender({Id})", id);

        var user = await userRequestContext.GetUserAsync();
        var token = userRequestContext.GetJwt();
        
        // reset render to rerendering state
        var render = await rendersService.MarkAsRerendering(id);
        
        // fetch image details from typo cloud
        var cloudImage = await typoCloudService.GetCloudImage(user.TypoId, token, render.CloudId);
        
        // run rerender in background, will set to render finished when done
        renderTaskDispatcherService.EnqueueRerender(new RerenderRequestData(
            render,
            renderParametersDto,
            cloudImage
        ));
        
        return mapper.Map<RenderInfoDto>(render);
    }
}