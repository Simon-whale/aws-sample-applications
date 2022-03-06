using System.Net;
using System.Runtime.InteropServices.ComTypes;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using WebApplication1.Interfaces;

namespace WebApplication1.Services;

public class S3Service : IS3Service
{
    private readonly AmazonS3Client _client;
    
    public S3Service(IConfiguration configuration)
    {
        _client = new AmazonS3Client(
            new AmazonS3Config()
            {
                ServiceURL = configuration["awsServer"],
                ForcePathStyle = true
            });
    }

    public async Task<string> CreateBucket(string name)
    {
        var buckets = await GetBuckets();
        var exists = buckets.Buckets.FirstOrDefault(p => p.BucketName == name);

        if (exists != null) return $"Bucket {name} already exists";
        var newBucket = await _client.PutBucketAsync(
            new PutBucketRequest
            {
                BucketName = name,
                UseClientRegion = true
            }
        );

        return $"Bucket {name} has been created";
    }

    public async Task<List<string>> ListBuckets()
    {
        var buckets = await GetBuckets();
        return buckets.Buckets.Select(p => p.BucketName).ToList();
    }

    public async Task<string> RemoveBucket(string name)
    {
        var buckets = await GetBuckets();
        var exists = buckets.Buckets.FirstOrDefault(p => p.BucketName == name);

        if (exists == null) return $"Bucket {name} does not exists";

        var response = _client.DeleteBucketAsync(new DeleteBucketRequest()
        {
            BucketName = name,
            UseClientRegion = true
        });
        
        return $"Bucket {name} was removed successfully";
    }

    public async Task<string> UploadFile(IFormFile file, string bucketName)
    {
        var bucket = await GetBuckets();
        var exists = bucket.Buckets.FirstOrDefault(p => p.BucketName == bucketName);

        if (exists != null) return $"Bucket {bucketName} not found";
        
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = memoryStream,
                Key = file.FileName,
                BucketName = bucketName
            };

            var fileTransfer = new TransferUtility(_client);
            await fileTransfer.UploadAsync(uploadRequest);
        }

        return "File upload Completed";
    }

    public async Task<List<string>> GetFilesInBucket(string bucketName)
    {
        var files = await _client.ListObjectsAsync(
            new ListObjectsRequest
            {
                BucketName  = bucketName
            });

        var response = files.S3Objects.Select(p => p.Key);
        return response.ToList();
    }

    public async Task<string> DeleteFile(string filename, string bucketName)
    {
        await _client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = filename
        });

        return $"file {filename} was removed";
    }

    public async Task<string> DeleteBucket(string bucketName)
    {
        try
        {
            var buckets = await GetBuckets();
            var exists = buckets.Buckets.FirstOrDefault(p => p.BucketName == bucketName);

            if (exists != null) return $"unable to remove bucker {bucketName}";
            
            var removeBucket = new DeleteBucketRequest
            {
                BucketName = bucketName, 
                UseClientRegion = true
            };

            var response = await _client.DeleteBucketAsync(removeBucket);
            return $"bucket {bucketName} removed";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
    private Task<ListBucketsResponse> GetBuckets() => _client.ListBucketsAsync();
}