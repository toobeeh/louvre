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
    UserService userService,
    TypoApiClientService typoApiClientService
    ) : ControllerBase
{
    
    [HttpGet, Authorize]
    public async Task<IEnumerable<AuthorizedUserDto>> GetAuthorizedUsers()
    {
        return await userService.GetAllUsers();
    }
    
    [HttpPost, Authorize(Roles = "Administrator,Moderator")]
    public async Task<AuthorizedUserDto> AuthorizeUser(AuthorizeUserDto userDto)
    {
        if (userDto.UserType == UserTypeEnum.Administrator)
        {
            throw new ArgumentException("Administrators are recognized via typo member flags and cannot be added here.");
        }

        // fetch public user details from typo api to get name
        var user = await typoApiClientService
            .GetClient((url, client) => new MembersControllerClient(url, client))
            .GetPublicMemberInfoByLoginAsync(Convert.ToDouble(userDto.Login));
        
        return await userService.AddUser(new AuthorizedUserDto(userDto.Login, userDto.UserType, user.UserName));
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
        await userService.DeleteUserByLogin(id);

        return NoContent(); // 204 No Content
    }
}