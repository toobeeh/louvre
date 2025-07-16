using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using toobeeh.Louvre.Server.Config;
using toobeeh.Louvre.Server.Database;
using toobeeh.Louvre.Server.Database.Model;
using toobeeh.Louvre.Server.Dto;
using toobeeh.Louvre.TypoApiClient;

namespace toobeeh.Louvre.Server.Service;

public class AuthorizationService(
    ILogger<AuthorizationService> logger, 
    HttpClient httpClient, 
    IOptions<TypoApiConfig> apiOptions,
    AuthorizedUserCacheService authorizedUserCacheService,
    AppDatabaseContext dbContext
    )
{
    private readonly MembersControllerClient _membersClient = new(apiOptions.Value.BaseUrl, httpClient);

    /// <summary>
    /// Gets the authorized user based on the provided access token.
    /// Caches the user for future requests to avoid unnecessary API calls.
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns>
    /// Dto that contains the user login (id) and type.
    /// </returns>
    public async Task<AuthorizedUserDto> GetAuthorizedUser(string accessToken)
    {
        logger.LogTrace("GetAuthorizedUser({accessToken})", accessToken);
        
        // check if user already in cache
        var cachedUser = authorizedUserCacheService.GetUserByToken(accessToken);
        
        if(cachedUser is not null)
        {
            logger.LogDebug("User found in cache: {cachedUser}", cachedUser);
            return cachedUser;
        }
        
        // make request on behalf of the user to typo api to fetch member details
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var member = await _membersClient.GetAuthenticatedMemberAsync();
        
        // if member is typo admin, always authorize
        if (member.MemberFlags.Contains(MemberFlags.Admin))
        {
            logger.LogDebug("User is an admin, authorizing without db check");
            var adminUser = new AuthorizedUserDto(member.UserLogin, UserTypeEnum.Administrator, member.UserName);
            authorizedUserCacheService.CacheUser(accessToken, adminUser);
            return adminUser;
        }
        
        // fetch user config from db
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == member.UserLogin);
        if (user is null)
        {
            throw new AuthenticationException("User not authorized to use app");
        }
        
        var authorizedUser = new AuthorizedUserDto(member.UserLogin, UserTypeEnum.Contributor, member.UserName);
        authorizedUserCacheService.CacheUser(accessToken, authorizedUser);
        
        logger.LogDebug("User fetched and cached for future requests");

        return authorizedUser;
    }
}