using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using toobeeh.Louvre.Server.Authentication;
using toobeeh.Louvre.Server.Database;
using toobeeh.Louvre.Server.Database.Model;
using toobeeh.Louvre.Server.Dto;
using toobeeh.Louvre.Server.Service;
using toobeeh.Louvre.TypoApiClient;

namespace toobeeh.Louvre.Server.Controllers;

[ApiController]
[Route("users")]
public class UsersController(
    ILogger<UsersController> logger,
    AppDatabaseContext dbContext,
    TypoApiClientService typoApiClientService
    ) : ControllerBase
{
    
    [HttpGet]
    public async Task<IEnumerable<AuthorizedUserDto>> GetAuthorizedUsers()
    {
        return await dbContext.Users
            .Select(u => new AuthorizedUserDto(u.Id, u.Type, u.Name))
            .ToListAsync();
    }
    
    [HttpPost, Authorize(Roles = "Administrator,Moderator")]
    public async Task<AuthorizedUserDto> AuthorizeUser(AuthorizeUserDto userDto)
    {
        if (userDto.UserType == UserTypeEnum.Administrator)
        {
            throw new ArgumentException("Administrators are recognized via typo member flags and cannot be added here.");
        }

        var user = await typoApiClientService
            .GetClient((url, client) => new MembersControllerClient(url, client))
            .GetPublicMemberInfoByLoginAsync(Convert.ToDouble(userDto.Login));

        dbContext.Users.Add(new UserEntity() { Id = userDto.Login, Type = userDto.UserType, Name = user.UserName});
        await dbContext.SaveChangesAsync();
        
        return new AuthorizedUserDto(userDto.Login, userDto.UserType, user.UserName);
    }
    
    [HttpGet("me")]
    public Task<AuthorizedUserDto> GetCurrentUser()
    {
        var user = TypoAuthenticationHelper.FromPrincipal(User);
        return Task.FromResult(user);
    }
    
    [HttpDelete("{id}"), Authorize(Roles = "Administrator,Moderator")]
    public async Task<IActionResult> DeleteAuthorizedUser(string id)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);
        if (user == null)
        {
            return NotFound(new { Message = $"User with ID {id} not found." });
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }
}