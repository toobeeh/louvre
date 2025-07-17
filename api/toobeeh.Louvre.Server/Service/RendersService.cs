using Microsoft.EntityFrameworkCore;
using toobeeh.Louvre.Server.Database;
using toobeeh.Louvre.Server.Dto;

namespace toobeeh.Louvre.Server.Service;

public class RendersService(ILogger<RendersService> logger, AppDatabaseContext db)
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
            .Select(render => new RenderInfoDto(
                Ulid.Parse(render.Info.Id),
                render.Info.ApprovedTitle ?? render.Info.Title,
                render.User?.Name ?? render.Info.Drawer,
                render.Info.ApprovedTitle is not null,
                render.User is not null))
            .OrderByDescending(render => render.Id)
            .ToList();
    }
}