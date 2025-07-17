using Microsoft.EntityFrameworkCore;
using Minio.Exceptions;
using tobeh.Louvre.Server.Database;
using tobeh.Louvre.Server.Database.Model;
using tobeh.Louvre.Server.Dto;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Service;

public class RendersService(ILogger<RendersService> logger, AppDatabaseContext db, StorageService storageService)
{
    public async Task<IEnumerable<RenderInfoDto>> FindRenders(FindRendersFilterDto filter)
    {
        logger.LogTrace("FindRenders({Filter})", filter);
        
        var query = db.Renders.AsQueryable();
        
        // filter name contains
        if (!string.IsNullOrEmpty(filter.NameIncludeQuery))
        {
            query = query.Where(r => r.Title.Contains(filter.NameIncludeQuery));
        }
        
        // filter if render has finished
        if (filter.Rendered.HasValue)
        {
            query = query.Where(r => r.Rendered == filter.Rendered.Value);
        }
        
        // filter if drawer name contains
        if (!string.IsNullOrEmpty(filter.DrawerName))
        {
            query = query.Where(r => r.Drawer.Contains(filter.DrawerName));
        }
        
        // filter if drawer login is approved
        if (!string.IsNullOrEmpty(filter.ApprovedDrawerLogin))
        {
            query = query.Where(r => r.ApprovedDrawerLogin == filter.ApprovedDrawerLogin);
        }
        
        // filter by language
        if (!string.IsNullOrEmpty(filter.Language))
        {
            query = query.Where(r => r.Language == filter.Language);
        }

        // join with users to get drawer name
        var results = await query
            .GroupJoin(db.Users,
                render => render.ApprovedDrawerLogin,
                user => user.Id,
                (render, user) => new { render, user })
            .SelectMany(
                render => render.user.DefaultIfEmpty(),
                (render, user) => new { Info = render.render, User = user })
            .ToListAsync();

        // map to dto and order
        return results
            .Select(render => this.MapToDto(render.Info, render.User?.Name))
            .OrderByDescending(render => render.Id)
            .ToList();
    }

    public async Task<RenderInfoDto> AddRenderRequest(CloudImageDto image, string ownerLogin)
    {
        logger.LogTrace("AddRenderRequest({Image})", image);

        var id = Ulid.NewUlid();
        
        var entity = db.Renders.Add(new RenderEntity()
        {
            Id = id.ToString(),
            Title = image.Name,
            ApprovedTitle = null,
            Approved = false,
            Drawer = image.Author,
            ApprovedDrawerLogin = null,
            Language = "", // TODO
            Rendered = false,
            OwnerCloudId = image.Id,
            CloudOwnerLogin = ownerLogin,
            RenderDuration = null,
            RenderFps = null,
            RenderOptimization = null
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new PreconditionFailedException("Render request already exists.", e);
        }

        return MapToDto(entity.Entity, null);
    }

    public async Task<RenderInfoDto> SetRenderCompleted(RenderingService.GifRenderResult gif)
    {
        logger.LogTrace("SetRenderCompleted({Gif})", gif);
        
        var render = db.Renders.FirstOrDefault(r => r.Id == gif.RenderId.ToString());
        if (render == null)
        {
            logger.LogWarning("Render not found for completion: {RenderId}", gif.RenderId);
            throw new InvalidOperationException($"Render with ID {gif.RenderId} not found.");
        }
        
        render.Rendered = true;
        render.RenderDuration = gif.Duration;
        render.RenderFps = gif.Fps;
        render.RenderOptimization = gif.Optimization;
        
        var entity = db.Renders.Update(render);
        await db.SaveChangesAsync();
        
        var drawerName = string.IsNullOrEmpty(render.ApprovedDrawerLogin)
            ? null
            : db.Users.FirstOrDefault(u => u.Id == render.ApprovedDrawerLogin)?.Name;
        
        return MapToDto(entity.Entity, drawerName);
    }
    
    private RenderInfoDto MapToDto(RenderEntity render, string? drawerName)
    {
        return new RenderInfoDto(
            Ulid.Parse(render.Id),
            render.ApprovedTitle ?? render.Title,
            drawerName ?? render.Drawer,
            render.ApprovedTitle is not null,
            drawerName is not null,
            render.Rendered,
            storageService.GetUrlForBucket(StorageService.GifBucketName, $"{render.Id}.gif"),
            storageService.GetUrlForBucket(StorageService.ThumbnailBucketName, $"{render.Id}.png")
        );
    }
}