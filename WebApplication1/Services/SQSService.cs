using System.Net;
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using WebApplication1.Interfaces;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class SQSService : ISQSService
{
    private readonly AmazonSQSClient _client;
    
    public SQSService(IConfiguration configuration)
    {
        _client = new AmazonSQSClient(new AmazonSQSConfig
        {
            ServiceURL = configuration["awsServer"]
        });
    }

    public async Task<string> CreateQueue(string queueName)
    {
        var response = await _client.CreateQueueAsync(new CreateQueueRequest
        {
            QueueName = queueName
        });

        return $"Queue {queueName} has been created";
    }
    
    public async Task<List<QueueDetails>> GetQueues()
    {
        var response = new List<QueueDetails>();
        
        var queueRequest = await _client.ListQueuesAsync(new ListQueuesRequest());
        foreach (var queueUrl in queueRequest.QueueUrls)
        {
            GetQueueAttributesResponse attributesResponse = await _client.GetQueueAttributesAsync(new GetQueueAttributesRequest(queueUrl, new List<string> { "All" }));
            response.Add(new QueueDetails() {QueueUrl = queueUrl, Messages = attributesResponse.ApproximateNumberOfMessages});
        }

        return response;
    }

    public async Task<string> PostMessage(string queueName, string message)
    {
        var queueUrl = await GetQueueUrl(queueName);
        await _client.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message)
        });

        return "Message has been sent";
    }

    public async Task<string> GetMessage(string queueName)
    {
        var queueUrl = await GetQueueUrl(queueName);
        var response = await _client.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            WaitTimeSeconds = 15,
            AttributeNames = new List<string> {"ApproximateRecieveCount"},
            MessageAttributeNames = new List<string> { "All"} 
        });

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            var output = JsonSerializer.Serialize(response.Messages);
            return output;
        }

        return "No messages to be returned";
    }

    public async Task<string> RemoveMessage(string queueName, string messageHandle)
    {
        var queueUrl = await GetQueueUrl(queueName);
        var response = await _client.DeleteMessageAsync(new DeleteMessageRequest
        {
            QueueUrl = queueUrl,
            ReceiptHandle = messageHandle
        });

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return 
                "Failed to DeleteMessageAsync with for [{messageHandle}] from queue '{queueName}'. Response: {response.HttpStatusCode}";

        return $"Message has been deleted from {queueName}";
    }

    public async Task<string> DeleteQueue(string queueName)
    {
        var queueUrl = await GetQueueUrl(queueName);
        var response = await _client.DeleteQueueAsync(new DeleteQueueRequest
        {
            QueueUrl = queueUrl
        });

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return $"Unable to remove Queue {queueName}";
            
        return $"Queue {queueName} has been removed";
    }
    
    private async Task<string> GetQueueUrl(string queue)
    {
        var request = new GetQueueUrlRequest(queue);
        var response = await _client.GetQueueUrlAsync(request);
        if (response.HttpStatusCode != HttpStatusCode.OK)
            return String.Empty;

        return response.QueueUrl;
    }
}