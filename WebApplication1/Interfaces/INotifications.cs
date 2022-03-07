using System.Net;

namespace WebApplication1.Interfaces;

public interface INotifications
{
    Task<IEnumerable<string>> GetTopics();
    Task<HttpStatusCode> CreateTopic(string topicName);
    Task<bool> AddMessageToTopic(string topicArn, string message);
    Task<HttpStatusCode> RemoveTopic(string topicArn);
}