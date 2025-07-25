using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Service;

public class TypoCloudService(
    ILogger<TypoCloudService> logger,
    HttpClient httpClient,
    TypoApiClientService typoApiClientService)
{
    public async Task<CloudImageDto> GetCloudImage(double typoId, string accessToken, string id)
    {
        logger.LogTrace("GetCloudImage({typoId}, {AccessToken}, {Id})", typoId, accessToken, id);
        
        var client = await typoApiClientService.GetClient((url, client) => new CloudControllerClient(url, client));
        return await client.GetImageFromUserCloudAsync(typoId, id);
    }

    public async Task<double[][]> GetSkdFromCloud(CloudImageDto image)
    {
        logger.LogTrace("GetSkdFromCloud({image})", image);
        
        var response = await httpClient.GetAsync(image.CommandsUrl);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to fetch SKD from cloud: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch SKD from cloud: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<double[][]>();
        return result ?? throw new InvalidOperationException("Failed to deserialize SKD from cloud response.");
    }

    public async Task<byte[]> GetImageFromCloud(CloudImageDto image)
    {
        logger.LogTrace("GetImageFromCloud({image})", image);
        
        var response = await httpClient.GetAsync(image.ImageUrl);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to fetch image from cloud: {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"Failed to fetch image from cloud: {response.StatusCode}");
        }
        
        return await response.Content.ReadAsByteArrayAsync();
    }
}