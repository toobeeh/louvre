using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using tobeh.Louvre.Server.Service;

namespace tobeh.Louvre.Server.Authentication;

public class TypoTokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    AuthorizationService authorizationService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public new static readonly string Scheme = "TypoToken";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogTrace("HandleAuthenticateAsync()");
        
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        var authHeader = Request.Headers["Authorization"].ToString();

        if (!authHeader.StartsWith("Bearer "))
            return AuthenticateResult.Fail("Invalid Scheme");

        var token = authHeader["Bearer ".Length..].Trim();
        var user = await authorizationService.GetAuthorizedUser(token);
        return AuthenticateResult.Success(TypoAuthenticationHelper.CreateTicket(user, token));
    }
}