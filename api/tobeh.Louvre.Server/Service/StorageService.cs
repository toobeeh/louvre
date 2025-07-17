using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using tobeh.Louvre.Server.Config;

namespace tobeh.Louvre.Server.Service;

public class StorageService(
    ILogger<StorageService> logger,
    IMinioClient minioClient,
    IOptions<S3Config> options)
{
    public static readonly string GifBucketName = "gifs";
    public static readonly string ThumbnailBucketName = "thumbnails";
    
    public async Task SaveGif(Ulid id, byte[] gif)
    {
        logger.LogTrace("SaveGif({Id})", id);
        
        var stream = new MemoryStream(gif);
        
        var put = new PutObjectArgs()
            .WithBucket(GifBucketName)
            .WithObject($"{id.ToString()}.gif")
            .WithStreamData(stream)
            .WithContentType("image/gif")
            .WithObjectSize(stream.Length);
        await minioClient.PutObjectAsync(put);
    }
    
    public async Task SaveThumbnail(Ulid id, byte[] thumbnail)
    {
        logger.LogTrace("SaveThumbnail({Id})", id);
        
        var stream = new MemoryStream(thumbnail);
        
        var put = new PutObjectArgs()
            .WithBucket(ThumbnailBucketName)
            .WithObject($"{id.ToString()}.png")
            .WithStreamData(stream)
            .WithContentType("image/png")
            .WithObjectSize(stream.Length);
        await minioClient.PutObjectAsync(put);
    }
    
    public string GetUrlForBucket(string bucketName, string objectName)
    {
        logger.LogTrace("GetUrlInBucket({BucketName}, {ObjectName})", bucketName, objectName);

        return $"{options.Value.ExternalBaseUrl}/{bucketName}/{objectName}";;
    }
}