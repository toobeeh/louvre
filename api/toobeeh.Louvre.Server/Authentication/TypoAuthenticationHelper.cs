using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using toobeeh.Louvre.Server.Database.Model;
using toobeeh.Louvre.Server.Dto;

namespace toobeeh.Louvre.Server.Authentication;

public class TypoAuthenticationHelper
{
    public static AuthenticationTicket CreateTicket(AuthorizedUserDto user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, user.UserType.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Login)
        };
        
        var identity = new ClaimsIdentity(claims, TypoTokenAuthenticationHandler.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TypoTokenAuthenticationHandler.Scheme);
        return ticket;
    }
    
    public static AuthorizedUserDto FromPrincipal(ClaimsPrincipal principal)
    {
        if (principal == null || !principal.Identity.IsAuthenticated)
        {
            throw new ArgumentException("Invalid principal");
        }

        var login = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var parsed = Enum.TryParse<UserTypeEnum>(principal.FindFirst(ClaimTypes.Role)?.Value, out var parsedUserType);

        if (!parsed)
        {
            throw new ArgumentException("Invalid user type in principal");
        }

        return new AuthorizedUserDto(login ?? string.Empty, parsedUserType);
    }
}