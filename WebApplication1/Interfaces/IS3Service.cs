namespace WebApplication1.Interfaces;

public interface IS3Service
{
    Task<string> CreateBucket(string name);
    Task<List<string>> ListBuckets();
    Task<string> RemoveBucket(string name);
    Task<string> UploadFile(IFormFile file, string BucketName);
    Task<List<string>> GetFilesInBucket(string bucketName);
    Task<string> DeleteFile(string filename, string bucketName);
    Task<string> DeleteBucket(string bucketName);

}