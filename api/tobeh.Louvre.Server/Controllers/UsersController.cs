using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tobeh.Louvre.Server.Controllers.Dto;
using tobeh.Louvre.Server.Database.Model;
using tobeh.Louvre.Server.Service;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Controllers;

[ApiController]
[Route("users")]
public class UsersController(
    ILogger<UsersController> logger,
    UsersService usersService,
    TypoApiClientService typoApiClientService,
    UserRequestContext userRequestContext
    ) : ControllerBase
{
    
    [HttpGet, Authorize]
    public async Task<IEnumerable<UserDto>> GetAuthorizedUsers()
    {
        logger.LogTrace("GetAuthorizedUsers()");
        
        return await usersService.GetAllUsers();
    }
    
    [HttpPost, Authorize(Policy = "Role:Moderator")]
    public async Task<UserDto> AuthorizeUser(AuthorizeUserDto userDto)
    {
        logger.LogTrace("AuthorizeUser({UserDto})", userDto);
        
        if (userDto.UserType == UserTypeEnum.Administrator)
        {
            throw new ArgumentException("Administrators are recognized via typo member flags and cannot be added here.");
        }

        // fetch public user details from typo api to get name
        var user = await (await typoApiClientService
                .GetClient((url, client) => new MembersControllerClient(url, client)))
            .GetPublicMemberInfoByDiscordIdAsync(userDto.DiscordId);
        
        return await usersService.AddUser(new UserDto(Convert.ToInt32(user.TypoId), userDto.UserType, user.UserName));
    }
    
    [HttpGet("me"), Authorize]
    public async Task<UserDto> GetCurrentUser()
    {
        logger.LogTrace("GetCurrentUser()");
        
        return await userRequestContext.GetUserAsync();
    }
    
    [HttpDelete("{id}"), Authorize(Policy = "Role:Moderator")]
    public async Task<IActionResult> DeleteAuthorizedUser(string id)
    {
        logger.LogTrace("DeleteAuthorizedUser({Id})", id);
        
        await usersService.DeleteUserByLogin(id);

        return NoContent(); // 204 No Content
    }
    
    [HttpPatch("{id:int}/rename"), Authorize(Policy = "Role:Moderator")]
    public async Task<UserDto> RenameUser(int id, RenameUserDto renameDto)
    {
        logger.LogTrace("RenameUser({Id}, {NewName})", id, renameDto);
        
        var user = await usersService.RenameUser(Convert.ToInt32(id), renameDto.NewName);

        return user;
    }
    
    [HttpPatch("{id:int}/promote"), Authorize(Policy = "Role:Moderator")]
    public async Task<UserDto> PromoteUser(int id, PromoteUserDto promoteDto)
    {
        logger.LogTrace("PromoteUserDto({Id}, {newType})", id, promoteDto);

        if (promoteDto.NewUserType == UserTypeEnum.Administrator)
        {
            throw new ArgumentException("Administrators are recognized via typo member flags and cannot be set.");
        }

        var requestingUser = await userRequestContext.GetUserAsync();
        var targetUser = await usersService.GetUserByTypoId(id);
        if (targetUser == null)
        {
            throw new Exception($"User with typoId {id} not found.");
        }
        
        if(requestingUser.UserType == UserTypeEnum.Moderator && targetUser.UserType == UserTypeEnum.Administrator)
        {
            throw new ArgumentException("Moderators cannot manage other moderators.");
        }
        
        var user = await usersService.ChangeUserType(id, promoteDto.NewUserType);

        return user;
    }
    
}