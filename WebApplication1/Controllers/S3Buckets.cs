using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class S3Buckets : Controller
{
    private readonly AmazonS3Client _client;

    public S3Buckets()
    {
        _client = new AmazonS3Client(
            new AmazonS3Config()
            {
                ServiceURL = "http://localhost:4566",
                ForcePathStyle = true
            });
    }
    
    [HttpGet]
    public async Task<IActionResult> CreateBucket(string Name)
    {
        var buckets = await GetBuckets();
        var exists = buckets.Buckets.FirstOrDefault(p => p.BucketName == Name);

        if (exists != null) return Ok($"Bucket {Name} already exists");
        
        var bucket = await _client.PutBucketAsync(
            new PutBucketRequest
            {
                BucketName = Name,
                UseClientRegion = true
            }
        );

        return Ok($"Bucket {Name} has been created");
    }

    [HttpGet]
    public async Task<IActionResult> ListBuckets()
    {
        var buckets = await GetBuckets();
        var response = buckets.Buckets.Select(p => p.BucketName).ToList(); 
        return Ok(JsonSerializer.Serialize(response));
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveBucket(string Name)
    {
        var buckets = await GetBuckets();
        var exists = buckets.Buckets.FirstOrDefault(p => p.BucketName == Name);

        if (exists == null) return Ok($"Bucket {Name} does not exists");

        var response = _client.DeleteBucketAsync(new DeleteBucketRequest()
        {
            BucketName = Name,
            UseClientRegion = true
        });

        return Ok($"Bucket {Name} was removed successfully");
    }

    [HttpPost]
    public async Task<IActionResult> UploadFileToS3(IFormFile file, string BucketName)
    {
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = memoryStream,
                Key = file.FileName,
                BucketName = BucketName
            };

            var fileTransfer = new TransferUtility(_client);
            await fileTransfer.UploadAsync(uploadRequest);
        }

        return Ok("File upload Completed");
    }

    [HttpGet]
    public async Task<IActionResult> GetFilesInBucket(string bucketName)
    {
        var files = await _client.ListObjectsAsync(
            new ListObjectsRequest
            {
                BucketName  = bucketName
            });

        var response = files.S3Objects.Select(p => p.Key);
        return Ok(JsonSerializer.Serialize(response));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFile(string fileName, string bucketName)
    {

        var response = await _client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = fileName
        });

        return Ok($"file {fileName} was removed");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteBucket(string bucketName)
    {
        var removeBucket = new DeleteBucketRequest
        {
            BucketName = bucketName, 
            UseClientRegion = true
        };

        var response = await _client.DeleteBucketAsync(removeBucket);
        return Ok(response.HttpStatusCode);
    }
    
    private Task<ListBucketsResponse> GetBuckets()
    {
      return _client.ListBucketsAsync();  
    } 
}