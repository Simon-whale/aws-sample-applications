using WebApplication1.Models;

namespace WebApplication1.Interfaces;

public interface ISQSService
{
    Task<List<QueueDetails>> GetQueues();
    Task<string> CreateQueue(string queueName);
    Task<string> PostMessage(string queueName, string message);
    Task<string> GetMessage(string queueName);
    Task<string> RemoveMessage(string queueName, string messageHandle);
    Task<string> DeleteQueue(string queueName);
}