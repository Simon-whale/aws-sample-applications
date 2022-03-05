using System.Net;
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class QueueMessages : Controller
{
    //Todo: Add Deadletter queue example
    //Todo: refactor exception types
    
    private readonly AmazonSQSClient _client;

    public QueueMessages()
    {
        _client = new AmazonSQSClient(new AmazonSQSConfig
        {
            ServiceURL = "http://localhost:4566"
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateQueueRequest(string queueName)
    {
        try
        {
            var queueRequest = new CreateQueueRequest()
            {
                QueueName = queueName,
            };

            var response = await _client.CreateQueueAsync(queueRequest);
            return Ok($"Queue {queueName} has been created");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetQueues()
    {
        var response = new List<QueueDetails>();
        
        var queueRequest = await _client.ListQueuesAsync(new ListQueuesRequest());
        foreach (var queueURL in queueRequest.QueueUrls)
        {
            GetQueueAttributesResponse attributesResponse = await _client.GetQueueAttributesAsync(new GetQueueAttributesRequest(queueURL, new List<string> { "All" }));
            response.Add(new QueueDetails() {QueueUrl = queueURL, Messages = attributesResponse.ApproximateNumberOfMessages});
        }

        var json = JsonSerializer.Serialize(response);
        return Ok(json);
    }

    [HttpPost]
    public async Task<IActionResult> PostMessage(string queueName, string message)
    {
        var queueUrl = await GetQueueUrl(queueName);

        try
        {
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = JsonSerializer.Serialize(message)
                // MessageAttributes = SqsMessageTypeAttribute.CreateAttribute<T>()
            };

            await _client.SendMessageAsync(sendMessageRequest);
            return Ok($"Message has been sent");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(string queueName)
    {
        try
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
                return Ok(output);
            }

            return Ok("No messages to be returned");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveMessage(string queueName, string messageHandle)
    {
        try
        {
            var queueUrl = await GetQueueUrl(queueName);
            var response = await _client.DeleteMessageAsync(new DeleteMessageRequest
            {
                QueueUrl = queueUrl,
                ReceiptHandle = messageHandle
            });

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return BadRequest(
                    $"Failed to DeleteMessageAsync with for [{messageHandle}] from queue '{queueName}'. Response: {response.HttpStatusCode}");

            return Ok($"Message has been deleted from {queueName}");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteQueue(string queueName)
    {
        try
        {
            var queueUrl = await GetQueueUrl(queueName);
            var response = await _client.DeleteQueueAsync(new DeleteQueueRequest
            {
                QueueUrl = queueUrl
            });

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return BadRequest($"Unable to remove Queue {queueName}");
            
            return Ok($"Queue {queueName} has been removed");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
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