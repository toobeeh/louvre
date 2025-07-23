using Microsoft.EntityFrameworkCore;
using Minio.Exceptions;
using tobeh.Louvre.Server.Controllers.Dto;
using tobeh.Louvre.Server.Database;
using tobeh.Louvre.Server.Database.Model;
using tobeh.Louvre.Server.Service.Data;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Service;

public class RendersService(ILogger<RendersService> logger, AppDatabaseContext db, StorageService storageService)
{
    public async Task<RenderData> GetRenderById(Ulid id)
    {
        logger.LogTrace("GetRenderById({Id})", id);
        
        var render = await db.Renders.FirstOrDefaultAsync(r => r.Id == id.ToString());
        if (render == null)
        {
            logger.LogWarning("Render not found: {Id}", id);
            throw new InvalidOperationException($"Render with ID {id} not found.");
        }

        var drawerName = render.ApprovedDrawerTypoId is null
            ? null
            : db.Users.FirstOrDefault(u => u.Id == render.ApprovedDrawerTypoId)?.Name;

        return MapToDto(render, drawerName);
    }
    
    public async Task<IEnumerable<RenderData>> FindRenders(FindRendersFilterDto filter)
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
        if (filter.ApprovedDrawerTypoId is not null)
        {
            query = query.Where(r => r.ApprovedDrawerTypoId == filter.ApprovedDrawerTypoId);
        }
        
        // filter by language
        if (!string.IsNullOrEmpty(filter.Language))
        {
            query = query.Where(r => r.Language == filter.Language);
        }

        // join with users to get drawer name
        var results = await query
            .GroupJoin(db.Users,
                render => render.ApprovedDrawerTypoId,
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

    public async Task<RenderData> AddRenderRequest(CloudImageDto image, double typoId)
    {
        logger.LogTrace("AddRenderRequest({typoId})", typoId);

        var id = Ulid.NewUlid();
        var intTypoId = Convert.ToInt32(typoId);
        
        var entity = db.Renders.Add(new RenderEntity()
        {
            Id = id.ToString(),
            Title = image.Name,
            ApprovedTitle = null,
            Approved = false,
            Drawer = image.Author,
            ApprovedDrawerTypoId = image.Own ? intTypoId : null,
            Language = image.Language,
            Rendered = false,
            OwnerCloudId = image.Id,
            CloudOwnerTypoId = intTypoId,
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

    public async Task<RenderData> MarkAsRerendering(Ulid id)
    {
        logger.LogTrace("MarkAsRerendering({Id})", id);
        
        var render = await db.Renders.FirstOrDefaultAsync(r => r.Id == id.ToString());
        if (render == null)
        {
            logger.LogWarning("Render not found for rerendering: {Id}", id);
            throw new InvalidOperationException($"Render with ID {id} not found.");
        }

        if (!render.Rendered)
        {
            logger.LogWarning("Tried to mark render as rerendering that has not rendered yet: {Id}", id);
            throw new InvalidOperationException("Cannot mark render as rerendering that has not been rendered yet.");
        }

        if (render.Approved)
        {
            logger.LogWarning("Tried to mark already approved render as rerendering: {Id}", id);
            throw new InvalidOperationException("Cannot mark an already approved render as rerendering.");
        }
        
        render.Rendered = false;
        render.RenderDuration = null;
        render.RenderFps = null;
        render.RenderOptimization = null;
        
        var entity = db.Renders.Update(render);
        await db.SaveChangesAsync();
        var drawerName = render.ApprovedDrawerTypoId is null
            ? null
            : db.Users.FirstOrDefault(u => u.Id == render.ApprovedDrawerTypoId)?.Name;
        
        return MapToDto(entity.Entity, drawerName);
    }

    public async Task<RenderData> SetRenderCompleted(GifRenderResultData gif)
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
        
        var drawerName = render.ApprovedDrawerTypoId is null
            ? null
            : db.Users.FirstOrDefault(u => u.Id == render.ApprovedDrawerTypoId)?.Name;
        
        return MapToDto(entity.Entity, drawerName);
    }

    public async Task RemoveRender(Ulid id)
    {
        logger.LogTrace("RemoveRender({Id})", id);
        
        var render = await db.Renders.FirstOrDefaultAsync(r => r.Id == id.ToString());
        if (render == null)
        {
            logger.LogWarning("Render not found for removal: {Id}", id);
            throw new InvalidOperationException($"Render with ID {id} not found.");
        }

        if (render.Rendered == false)
        {
            logger.LogWarning("Tried to remove render that has not rendered yet");
            throw new InvalidOperationException("Cannot remove render that has not been rendered yet.");
        }
        
        db.Renders.Remove(render);
        await db.SaveChangesAsync();
    }

    public async Task<RenderData> ProposeRenderDetails(Ulid id, int? drawerTypoId, string? title)
    {
        logger.LogTrace("ProposeRenderDetails({Id}, {drawerTypoId}, {Title})", id, drawerTypoId, title);

        if (drawerTypoId is null && title is null)
        {
            throw new ArgumentException("At least one of drawerLogin or title must be proposed.");
        }
        
        var render = await db.Renders.FirstOrDefaultAsync(r => r.Id == id.ToString());
        if (render == null)
        {
            logger.LogWarning("Render not found for proposal: {Id}", id);
            throw new InvalidOperationException($"Render with ID {id} not found.");
        }
        
        if(render.Approved)
        {
            logger.LogWarning("Tried to propose details for already approved render: {Id}", id);
            throw new InvalidOperationException("Cannot propose details for an already approved render.");
        }

        string? newDrawerName = null;
        if (drawerTypoId is not null)
        {
            // check if drawer exists
            var drawer = await db.Users.FirstOrDefaultAsync(u => u.Id == drawerTypoId);
            if (drawer == null)
            {
                logger.LogWarning("Drawer not found for proposal: {drawerTypoId}", drawerTypoId);
                throw new InvalidOperationException($"Drawer with login {drawerTypoId} not found.");
            }
            
            render.ApprovedDrawerTypoId = drawerTypoId;
            newDrawerName = drawer.Name;
        }
        
        if (title is not null)
        {
            render.ApprovedTitle = title;
        }
        
        var entity = db.Renders.Update(render);
        await db.SaveChangesAsync();
        
        return MapToDto(entity.Entity, newDrawerName);
    }
    
    public async Task<RenderData> ApproveRender(Ulid id)
    {
        logger.LogTrace("ApproveRender({Id})", id);
        
        var render = await db.Renders.FirstOrDefaultAsync(r => r.Id == id.ToString());
        if (render == null)
        {
            logger.LogWarning("Render not found for approval: {Id}", id);
            throw new InvalidOperationException($"Render with ID {id} not found.");
        }
        
        if (render.Approved)
        {
            logger.LogWarning("Tried to approve already approved render: {Id}", id);
            throw new InvalidOperationException("Render is already approved.");
        }

        render.Approved = true;
        
        var entity = db.Renders.Update(render);
        await db.SaveChangesAsync();
        
        var drawerName = render.ApprovedDrawerTypoId is null
            ? null
            : db.Users.FirstOrDefault(u => u.Id == render.ApprovedDrawerTypoId)?.Name;
        
        return MapToDto(entity.Entity, drawerName);
    }
    
    public async Task<RenderData> UnapproveRender(Ulid id)
    {
        logger.LogTrace("UnapproveRender({Id})", id);
        
        var render = await db.Renders.FirstOrDefaultAsync(r => r.Id == id.ToString());
        if (render == null)
        {
            logger.LogWarning("Render not found for approval: {Id}", id);
            throw new InvalidOperationException($"Render with ID {id} not found.");
        }
        
        if (!render.Approved)
        {
            logger.LogWarning("Tried to unapprove render that is not approved: {Id}", id);
            throw new InvalidOperationException("Render is not yet approved.");
        }

        render.Approved = false;
        
        var entity = db.Renders.Update(render);
        await db.SaveChangesAsync();
        
        var drawerName = render.ApprovedDrawerTypoId is null
            ? null
            : db.Users.FirstOrDefault(u => u.Id == render.ApprovedDrawerTypoId)?.Name;
        
        return MapToDto(entity.Entity, drawerName);
    }
    
    private RenderData MapToDto(RenderEntity render, string? drawerName)
    {
        return new RenderData(
            Ulid.Parse(render.Id),
            render.OwnerCloudId,
            render.ApprovedTitle ?? render.Title,
            drawerName ?? render.Drawer,
            render.ApprovedTitle is not null,
            drawerName is not null,
            render.Rendered,
            storageService.GetUrlForBucket(StorageService.GifBucketName, $"{render.Id}.gif"),
            storageService.GetUrlForBucket(StorageService.ThumbnailBucketName, $"{render.Id}.png"),
            render.RenderDuration,
            render.RenderFps,
            render.RenderOptimization,
            render.Approved,
            render.Language
        );
    }
}