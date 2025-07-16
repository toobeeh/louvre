using toobeeh.Louvre.Server.Dto;

namespace toobeeh.Louvre.Server.Service;

public class AuthorizedUserCacheService(ILogger<AuthorizedUserCacheService> logger)
{
    private readonly Dictionary<string, AuthorizedUserDto> _tokenCache = new ();
    
    public void CacheUser(string token, AuthorizedUserDto user)
    {
        logger.LogTrace("CacheUser({Token}, {User})", token, user);
        
        if (string.IsNullOrEmpty(token) || user == null)
        {
            throw new ArgumentException("Token and user cannot be null or empty.");
        }

        _tokenCache[token] = user;
    }
    
    public AuthorizedUserDto? GetUserByToken(string token)
    {
        logger.LogTrace("GetUserByToken({Token})", token);
        
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.");
        }

        _tokenCache.TryGetValue(token, out var user);
        return user;
    }
}