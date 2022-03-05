using System.Net;
using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class Notifications : Controller
{
    private readonly AmazonSimpleNotificationServiceClient _client;

    public Notifications()
    {
        _client = new AmazonSimpleNotificationServiceClient(new AmazonSimpleNotificationServiceConfig
        {
            ServiceURL = "http://localhost:4566"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTopics()
    {
        var response = await _client.ListTopicsAsync(new ListTopicsRequest());
        var output = response.Topics.Select(p => p.TopicArn);
        return Ok(JsonSerializer.Serialize(output));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTopic(string topicName)
    {
        var response  = await _client.CreateTopicAsync(new CreateTopicRequest
        {
            Name = topicName
        });

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return BadRequest($"Unable to create topic {topicName}");

        return Ok($"Topic {topicName} has been creeated");
    }

    [HttpPost]
    public async Task<IActionResult> AddMessageToTopic(string topicArn, string message)
    {
        try
        {
            var request = new PublishRequest
            {
                TopicArn = topicArn,
                Message = message
            };

            await _client.PublishAsync(request);
            return Ok($"Message was successfully added to topic {topicArn}");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
    }

    [HttpGet]
    [Obsolete("please review before allowing it to be used")]
    public async Task<IActionResult> SubscribeMessages(string topicArn)
    {
        return Ok("Nothing to see here");
    }
    
    [HttpDelete]
    public async Task<IActionResult> RemoveTopic(string topicArn)
    {
        var response = await _client.DeleteTopicAsync(new DeleteTopicRequest
        {
            TopicArn = topicArn
        });

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return BadRequest($"Unable to remove topic {topicArn}");

        return Ok($"Topic {topicArn} has been removed");
    }
}