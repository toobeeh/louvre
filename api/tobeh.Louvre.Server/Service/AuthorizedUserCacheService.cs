using tobeh.Louvre.Server.Controllers.Dto;

namespace tobeh.Louvre.Server.Service;

public class AuthorizedUserCacheService(ILogger<AuthorizedUserCacheService> logger)
{
    private record TimestampedUserEntry<TEntry>(DateTimeOffset Timestamp, TEntry Entry);
    private readonly Dictionary<string, TimestampedUserEntry<UserDto>> _tokenCache = new ();
    private readonly Dictionary<string, TimestampedUserEntry<string>> _delegateTokenCache = new ();
    
    public void CacheUser(string identifier, UserDto user)
    {
        logger.LogTrace("CacheUser({Token}, {Entry})", identifier, user);
        
        if (string.IsNullOrEmpty(identifier) || user == null)
        {
            throw new ArgumentException("Identifier and user cannot be null or empty.");
        }

        _tokenCache[identifier] = new TimestampedUserEntry<UserDto>(DateTimeOffset.UtcNow, user);
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
            return userValue.Entry;
        }
        
        if(user is not null) _tokenCache.Remove(token);
        return null;
    }

    public void CacheDelegateToken(string identifier, string delegateToken)
    {
        logger.LogTrace("CacheDelegateToken({Identifier}, {DelegateToken})", identifier, delegateToken);
        if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(delegateToken))
        {
            throw new ArgumentException("Identifier and delegate token cannot be null or empty.");
        }
        
        _delegateTokenCache[identifier] = new TimestampedUserEntry<string>(DateTimeOffset.UtcNow,delegateToken);
    }
    
    public string? GetDelegateToken(string identifier)
    {
        logger.LogTrace("GetDelegateToken({Identifier})", identifier);
        
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException("Identifier cannot be null or empty.");
        }

        _delegateTokenCache.TryGetValue(identifier, out var tokenEntry);
        
        if (tokenEntry is { } entry && !(entry.Timestamp < DateTimeOffset.UtcNow.AddMinutes(-5)))
        {
            return entry.Entry;
        }
        
        if(tokenEntry is not null) _delegateTokenCache.Remove(identifier);
        return null;
    }
}