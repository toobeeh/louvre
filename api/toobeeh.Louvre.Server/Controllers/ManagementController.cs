using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using toobeeh.Louvre.Server.Authentication;
using toobeeh.Louvre.Server.Dto;
using toobeeh.Louvre.Server.Service;

namespace toobeeh.Louvre.Server.Controllers;

[ApiController]
[Route("management")]
public class ManagementController(
    ILogger<ManagementController> logger,
    AuthorizationService authorizationService
    ) : ControllerBase
{
    
    [HttpGet("users")]
    public IEnumerable<AuthorizedUserDto> GetAuthorizedUsers()
    {
        return [];
    }
    
    [HttpGet("users/me"), Authorize]
    public Task<AuthorizedUserDto> GetCurrentUser()
    {
        var user = TypoAuthenticationHelper.FromPrincipal(User);
        return Task.FromResult(user);
    }
}