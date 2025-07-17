using Minio;
using Minio.DataModel.Args;
using toobeeh.Louvre.Server.Service;

namespace toobeeh.Louvre.Server.Host;

public class MinioBucketSetupService(ILogger<MinioBucketSetupService> logger, IMinioClient minioClient) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await CreateBucketIfNotExists(StorageService.GifBucketName);
        await CreateBucketIfNotExists(StorageService.ThumbnailBucketName);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task CreateBucketIfNotExists(string name)
    {
        logger.LogTrace("CreateBucketIfNotExists({Name})", name);
        
        var bucketExists = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(name));
        
        if (!bucketExists)
        {
            logger.LogInformation("Creating bucket: {Name}", name);
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(name));
        }
        else
        {
            logger.LogInformation("Bucket already exists: {Name}", name);
            return;
        }
        
        // make new bucket public readable
        var policy = $$"""
           {
               "Version": "2012-10-17",
               "Statement": [
                   {
                       "Effect": "Allow",
                       "Principal": {"AWS": ["*"]},
                       "Action": ["s3:GetObject"],
                       "Resource": ["arn:aws:s3:::{{name}}/*"]
                   }
               ]
           }
           """;
        
        await minioClient.SetPolicyAsync(new SetPolicyArgs()
            .WithBucket(name)
            .WithPolicy(policy));
    }
}