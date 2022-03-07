using System.Net;
using System.Text.Json;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class Notifications : Controller
{
    private readonly INotifications _notifications;
    public Notifications(IConfiguration configuration, INotifications notifications)
    {
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<IActionResult> GetTopics()
    {
        var response = await _notifications.GetTopics();
        return Ok(JsonSerializer.Serialize(response));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTopic(string topicName)
    {
        var response = await _notifications.CreateTopic(topicName);
        if (response != HttpStatusCode.OK)
            return BadRequest($"Unable to create topic {topicName}");
        
        return Ok($"Topic {topicName} has been creeated");
    }

    [HttpPost]
    public async Task<IActionResult> AddMessageToTopic(string topicArn, string message)
    {
        try
        {
            var response = await _notifications.AddMessageToTopic(topicArn, message);
            if (!response)
                return Ok($"Message was not added to topic {topicArn}");
            
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
        var response = await _notifications.RemoveTopic(topicArn);

        if (response != HttpStatusCode.OK)
            return BadRequest($"Unable to remove topic {topicArn}");

        return Ok($"Topic {topicArn} has been removed");
    }
}