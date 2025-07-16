using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using toobeeh.Louvre.Server.Authentication;
using toobeeh.Louvre.Server.Database;
using toobeeh.Louvre.Server.Database.Model;
using toobeeh.Louvre.Server.Dto;

namespace toobeeh.Louvre.Server.Controllers;

[ApiController]
[Route("management")]
public class ManagementController(
    ILogger<ManagementController> logger,
    AppDatabaseContext dbContext
    ) : ControllerBase
{
    
    [HttpGet("users")]
    public async Task<IEnumerable<AuthorizedUserDto>> GetAuthorizedUsers()
    {
        return await dbContext.Users
            .Select(u => new AuthorizedUserDto(u.Id, u.Type))
            .ToListAsync();
    }
    
    [HttpGet("users/me")]
    public Task<AuthorizedUserDto> GetCurrentUser()
    {
        var user = TypoAuthenticationHelper.FromPrincipal(User);
        return Task.FromResult(user);
    }
    
    [HttpPost("users"), Authorize(Roles = "Administrator,Moderator")]
    public async Task AuthorizeUser(AuthorizedUserDto userDto)
    {
        if (userDto.UserType == UserTypeEnum.Administrator)
        {
            throw new ArgumentException("Administrators are recognized via typo member flags.");
        }

        dbContext.Users.Add(new UserEntity() { Id = userDto.Login, Type = userDto.UserType });
        await dbContext.SaveChangesAsync();
    }
}