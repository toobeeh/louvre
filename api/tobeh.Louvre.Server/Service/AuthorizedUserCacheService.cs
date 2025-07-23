using tobeh.Louvre.Server.Controllers.Dto;

namespace tobeh.Louvre.Server.Service;

public class AuthorizedUserCacheService(ILogger<AuthorizedUserCacheService> logger)
{
    private record TimestampedUserEntry(DateTimeOffset Timestamp, UserDto User);
    private readonly Dictionary<string, TimestampedUserEntry> _tokenCache = new ();
    
    public void CacheUser(string identifier, UserDto user)
    {
        logger.LogTrace("CacheUser({Token}, {User})", identifier, user);
        
        if (string.IsNullOrEmpty(identifier) || user == null)
        {
            throw new ArgumentException("Identifier and user cannot be null or empty.");
        }

        _tokenCache[identifier] = new TimestampedUserEntry(DateTimeOffset.UtcNow, user);
    }
    
    public UserDto? GetUserByToken(string token)
    {
        logger.LogTrace("GetUserByToken({Token})", token);
        
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.");
        }

        _tokenCache.TryGetValue(token, out var user);
        
        if (user is { } userValue && !(userValue.Timestamp < DateTimeOffset.UtcNow.AddMinutes(-5)))
        {
            return userValue.User;
        }
        
        if(user is not null) _tokenCache.Remove(token);
        return null;
    }
}