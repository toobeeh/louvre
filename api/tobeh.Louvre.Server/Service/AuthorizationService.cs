using System.Security.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using tobeh.Louvre.Server.Config;
using tobeh.Louvre.Server.Controllers.Dto;
using tobeh.Louvre.Server.Database.Model;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Service;

public class AuthorizationService(
    ILogger<AuthorizationService> logger, 
    HttpClient httpClient, 
    IOptions<TypoApiConfig> apiOptions,
    AuthorizedUserCacheService authorizedUserCacheService,
    UsersService usersService
    )
{
    private readonly MembersControllerClient _membersClient = new(apiOptions.Value.BaseUrl, httpClient);

    /// <summary>
    /// Gets the authorized user based on the provided access token.
    /// Caches the user for future requests to avoid unnecessary API calls.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="jwt"></param>
    /// <returns>
    /// Dto that contains the user login (id) and type.
    /// </returns>
    public async Task<UserDto> GetAuthorizedUser(ClaimsPrincipal principal, string jwt)
    {
        logger.LogTrace("GetAuthorizedUser({principal})", principal);
        
        // check if user already in cache
        var identifier = principal.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        
        if(identifier is null)
        {
            throw new AuthenticationFailureException("User identifier not found in principal");
        }
        
        var cachedUser = authorizedUserCacheService.GetUserByToken(identifier.Value);
        
        if(cachedUser is not null)
        {
            logger.LogDebug("User found in cache: {cachedUser}", cachedUser);
            return cachedUser;
        }
        
        // make request on behalf of the user to typo api to fetch member details
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        var member = await _membersClient.GetAuthenticatedMemberAsync();
        
        // if member is typo admin, always authorize
        if (member.MemberFlags.Contains(MemberFlags.Admin))
        {
            logger.LogDebug("User is an admin, authorizing without db check");
            var adminUser = new UserDto(Convert.ToInt32(member.TypoId), UserTypeEnum.Administrator, member.UserName);
            authorizedUserCacheService.CacheUser(identifier.Value, adminUser);
            return adminUser;
        }
        
        // fetch user config from db
        var user = await usersService.GetUserByLogin(member.UserLogin);
        if (user is null)
        {
            throw new AuthenticationFailureException("User not authorized to use app");
        }
        
        var authorizedUser = new UserDto(Convert.ToInt32(member.TypoId), user.UserType, member.UserName);
        authorizedUserCacheService.CacheUser(identifier.Value, authorizedUser);
        
        logger.LogDebug("User fetched and cached for future requests");

        return authorizedUser;
    }
}