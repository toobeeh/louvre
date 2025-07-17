using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using toobeeh.Louvre.Server.Service;

namespace toobeeh.Louvre.Server.Authentication;

public class TypoTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public new static readonly string Scheme = "TypoToken";
    
    private readonly AuthorizationService _authorizationService;

    public TypoTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        AuthorizationService authorizationService
    ) : base(options, logger, encoder)
    {
        _authorizationService = authorizationService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogTrace("HandleAuthenticateAsync()");
        
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Missing Authorization Header");

        var authHeader = Request.Headers["Authorization"].ToString();

        if (!authHeader.StartsWith("Bearer "))
            return AuthenticateResult.Fail("Invalid Scheme");

        var token = authHeader["Bearer ".Length..].Trim();
        var user = await _authorizationService.GetAuthorizedUser(token);
        return AuthenticateResult.Success(TypoAuthenticationHelper.CreateTicket(user, token));
    }
}