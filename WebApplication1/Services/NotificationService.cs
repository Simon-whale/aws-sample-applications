using System.Net;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using WebApplication1.Interfaces;

namespace WebApplication1.Services;

public class NotificationService : INotifications
{
    private readonly AmazonSimpleNotificationServiceClient _client;

    public NotificationService(IConfiguration configuration)
    {
        _client = new AmazonSimpleNotificationServiceClient(new AmazonSimpleNotificationServiceConfig
        {
            ServiceURL = configuration["awsServer"]
        });
    }
    
    public async Task<IEnumerable<string>> GetTopics()
    {
        var response = await _client.ListTopicsAsync(new ListTopicsRequest());
        return response.Topics.Select(p => p.TopicArn);
    }

    public async Task<HttpStatusCode> CreateTopic(string topicName)
    {
        var response  = await _client.CreateTopicAsync(new CreateTopicRequest
        {
            Name = topicName
        });

        return response.HttpStatusCode;
    }

    public async Task<bool> AddMessageToTopic(string topicArn, string message)
    {
        var response = _client.PublishAsync(new PublishRequest
        {
            TopicArn = topicArn,
            Message = message
        });

        return response.IsCompletedSuccessfully;
    }

    public async Task<HttpStatusCode> RemoveTopic(string topicArn)
    {
        var response = await _client.DeleteTopicAsync(new DeleteTopicRequest
        {
            TopicArn = topicArn
        });

        return response.HttpStatusCode;
    }
}