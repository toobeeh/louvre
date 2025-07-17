using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tobeh.Louvre.Server.Authentication;
using tobeh.Louvre.Server.Database.Model;
using tobeh.Louvre.Server.Dto;
using tobeh.Louvre.Server.Service;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Controllers;

[ApiController]
[Route("users")]
public class UsersController(
    ILogger<UsersController> logger,
    UsersService usersService,
    TypoApiClientService typoApiClientService
    ) : ControllerBase
{
    
    [HttpGet, Authorize]
    public async Task<IEnumerable<AuthorizedUserDto>> GetAuthorizedUsers()
    {
        logger.LogTrace("GetAuthorizedUsers()");
        
        return await usersService.GetAllUsers();
    }
    
    [HttpPost, Authorize(Roles = "Administrator,Moderator")]
    public async Task<AuthorizedUserDto> AuthorizeUser(AuthorizeUserDto userDto)
    {
        logger.LogTrace("AuthorizeUser({UserDto})", userDto);
        
        if (userDto.UserType == UserTypeEnum.Administrator)
        {
            throw new ArgumentException("Administrators are recognized via typo member flags and cannot be added here.");
        }

        // fetch public user details from typo api to get name
        var user = await typoApiClientService
            .GetClient((url, client) => new MembersControllerClient(url, client))
            .GetPublicMemberInfoByLoginAsync(Convert.ToDouble(userDto.Login));
        
        return await usersService.AddUser(new AuthorizedUserDto(userDto.Login, userDto.UserType, user.UserName));
    }
    
    [HttpGet("me")]
    public Task<AuthorizedUserDto> GetCurrentUser()
    {
        logger.LogTrace("GetCurrentUser()");
        
        var user = TypoAuthenticationHelper.GetUserFromPrincipal(User);
        return Task.FromResult(user);
    }
    
    [HttpDelete("{id}"), Authorize(Roles = "Administrator,Moderator")]
    public async Task<IActionResult> DeleteAuthorizedUser(string id)
    {
        logger.LogTrace("DeleteAuthorizedUser({Id})", id);
        
        await usersService.DeleteUserByLogin(id);

        return NoContent(); // 204 No Content
    }
}