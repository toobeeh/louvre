using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using tobeh.Louvre.Server.Controllers.Dto;
using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Authentication;

public class TypoAuthenticationHelper
{
    public static AuthenticationTicket CreateTicket(UserDto user, string token)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, user.UserType.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Login),
            new Claim("typoAccessToken", token)
        };
        
        var identity = new ClaimsIdentity(claims, TypoTokenAuthenticationHandler.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TypoTokenAuthenticationHandler.Scheme);
        return ticket;
    }
    
    public static UserDto GetUserFromPrincipal(ClaimsPrincipal principal)
    {
        if (principal == null || !principal.Identity.IsAuthenticated)
        {
            throw new ArgumentException("Invalid principal");
        }

        var login = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        var parsed = Enum.TryParse<UserTypeEnum>(principal.FindFirst(ClaimTypes.Role)?.Value, out var parsedUserType);

        if (!parsed)
        {
            throw new ArgumentException("Invalid user type in principal");
        }

        return new UserDto(login ?? string.Empty, parsedUserType, name ?? string.Empty);
    }
    
    public static string GetAccessTokenFromPrincipal(ClaimsPrincipal principal)
    {
        if (principal == null || !principal.Identity.IsAuthenticated)
        {
            throw new ArgumentException("Invalid principal");
        }

        var token = principal.FindFirst("typoAccessToken")?.Value;
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Access token not found in principal");
        }

        return token;
    }
}