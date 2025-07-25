using Microsoft.AspNetCore.Mvc;
using tobeh.Louvre.Server.Service;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Controllers;

[ApiController]
[Route("cloud")]
public class CloudController(
    ILogger<CloudController> logger,
    TypoApiClientService typoApiClientService,
    UserRequestContext userRequestContext
    ) : ControllerBase
{
    
    [HttpPost("search")]
    public async Task<IEnumerable<CloudImageDto>> SearchUserCloud(CloudSearchDto search)
    {
        logger.LogTrace("SearchUserCloud({Search})", search);

        var user = await userRequestContext.GetUserAsync();
        var client = await typoApiClientService.GetClient((url, client) => new CloudControllerClient(url, client));
        return await client.SearchUserCloudAsync(user.TypoId, search);
    }
}