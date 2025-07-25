using IdentityModel.Client;
using Microsoft.Extensions.Options;
using tobeh.Louvre.Server.Config;

namespace tobeh.Louvre.Server.Service;

public class TypoApiClientService(
    ILogger<TypoApiClientService> logger, 
    HttpClient httpClient, 
    IOptions<TypoApiConfig> apiOptions,
    AuthorizedUserCacheService authorizedUserCacheService
    )
{
    private string? ExchangedAudienceJwt { get; set; }
    public string? OriginalAudienceJwt { get; set; }

    public async Task<TClient> GetClient<TClient>(Func<string, HttpClient, TClient> clientFactory)
        where TClient : class
    {
        logger.LogTrace("GetClient<{ClientType}>", typeof(TClient).Name);
        
        if (clientFactory == null)
        {
            throw new ArgumentNullException(nameof(clientFactory));
        }

        // if user provided a jwt for authorization
        if (OriginalAudienceJwt is not null)
        {
            
            // check if there is already a exchange token set for this instance (per request)
            if (ExchangedAudienceJwt is null)
            {
                
                // check if there is a delegate token cached from a previous request
                var cachedDelegateToken = authorizedUserCacheService.GetDelegateToken(OriginalAudienceJwt);

                if (cachedDelegateToken is not null)
                {
                    ExchangedAudienceJwt = cachedDelegateToken;
                }
                
                // exchange jwt token for this API audience for a token with audience of typo api
                else
                {
                    ExchangedAudienceJwt = await ExchangeJwtAudienceAsync(OriginalAudienceJwt);
                    authorizedUserCacheService.CacheDelegateToken(OriginalAudienceJwt, ExchangedAudienceJwt);
                }
            }
            
            // set auth header for all requests using this instance
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ExchangedAudienceJwt);
        }

        var baseUrl = apiOptions.Value.BaseUrl;
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException("Typo API base URL is not configured.");
        }

        return clientFactory(baseUrl, httpClient);
    }
    
    private async Task<string> ExchangeJwtAudienceAsync(string jwt)
    {
        logger.LogTrace("ExchangeJwtAudienceAsync({Jwt})", jwt);

        var client = new HttpClient();
        var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
        {
            Address = $"{apiOptions.Value.BaseUrl}/openid",
            Policy = new()
            {
                ValidateEndpoints = false,
            }
        });
        
        if (disco.IsError)
        {
            throw new Exception(disco.Error);
        }
        
        var tokenResponse = await client.RequestTokenAsync(new TokenRequest
        {
            Address = disco.TokenEndpoint,
            GrantType = IdentityModel.OidcConstants.GrantTypes.TokenExchange,
            ClientId = "5",
            Parameters =
            {
                { IdentityModel.OidcConstants.TokenRequest.ClientId, "5" },
                { IdentityModel.OidcConstants.TokenRequest.SubjectToken, jwt },
                { IdentityModel.OidcConstants.TokenRequest.SubjectTokenType, IdentityModel.OidcConstants.TokenTypeIdentifiers.Jwt },
                { IdentityModel.OidcConstants.TokenRequest.Audience, apiOptions.Value.BaseUrl }
            }
        });

        if (tokenResponse.IsError)
        {
            throw new Exception(tokenResponse.Error);
        }

        if (tokenResponse.AccessToken is null)
        {
            throw new InvalidOperationException("Access token is null in the token response.");
        }

        return tokenResponse.AccessToken;
    }
}