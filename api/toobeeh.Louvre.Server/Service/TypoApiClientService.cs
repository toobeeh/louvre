using Microsoft.Extensions.Options;
using toobeeh.Louvre.Server.Config;

namespace toobeeh.Louvre.Server.Service;

public class TypoApiClientService(
    ILogger<TypoApiClientService> logger, 
    HttpClient httpClient, 
    IOptions<TypoApiConfig> apiOptions
    )
{

    public TClient GetClient<TClient>(Func<string, HttpClient, TClient> clientFactory)
        where TClient : class
    {
        logger.LogTrace("GetClient<{ClientType}>", typeof(TClient).Name);
        
        if (clientFactory == null)
        {
            throw new ArgumentNullException(nameof(clientFactory));
        }

        var baseUrl = apiOptions.Value.BaseUrl;
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException("Typo API base URL is not configured.");
        }

        return clientFactory(baseUrl, httpClient);
    }
}