using System.Security.Authentication;
using System.Security.Claims;
using tobeh.Louvre.Server.Controllers.Dto;

namespace tobeh.Louvre.Server.Service;

public class UserRequestContext
{
    private record UserContext(ClaimsPrincipal Principal, string Jwt);
    private readonly AuthorizationService _authService;

    private UserContext? User { get; }
    
    public UserRequestContext(IHttpContextAccessor accessor, ILogger<UserRequestContext> logger, AuthorizationService authService)
    {
        _authService = authService; 
        
        var context = accessor.HttpContext;
        if (context is { User.Identity.IsAuthenticated: true })
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                throw new AuthenticationException("Authorization header is missing or invalid.");
            }
            var jwt = authHeader["Bearer ".Length..].Trim();
            User = new UserContext(context.User, jwt);
        }

        else
        {
            logger.LogDebug("UserRequestContext initialized without authenticated user.");
            User = null;
        }
    }

    public Task<UserDto> GetUserAsync()
    {
        if (User is null)
        {
            throw new AuthenticationException("User is not authenticated.");
        }

        return _authService.GetAuthorizedUser(User.Principal, User.Jwt);
    }
    
    public string GetJwt()
    {
        if (User is null)
        {
            throw new AuthenticationException("User is not authenticated.");
        }

        return User.Jwt;
    }
}