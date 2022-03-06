using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class S3Buckets : Controller
{
    private readonly IS3Service _s3Service;

    public S3Buckets(IS3Service s3Service) => _s3Service = s3Service;
    
    [HttpGet]
    public async Task<IActionResult> CreateBucket(string name)
    {
        var response = await _s3Service.CreateBucket(name);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> ListBuckets()
    {
        var response = await _s3Service.ListBuckets(); 
        return Ok(JsonSerializer.Serialize(response));
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveBucket(string name)
    {
        var response = await _s3Service.RemoveBucket(name);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> UploadFileToS3(IFormFile file, string bucketName)
    {
        var response = await _s3Service.UploadFile(file, bucketName);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetFilesInBucket(string bucketName)
    {
        var response = await _s3Service.GetFilesInBucket(bucketName);
        return Ok(JsonSerializer.Serialize(response));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFile(string fileName, string bucketName)
    {
        var response = await _s3Service.DeleteFile(fileName, bucketName);
        return Ok(response);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteBucket(string bucketName)
    {
        var response = await _s3Service.DeleteBucket(bucketName);
        return Ok(response);
    }
}