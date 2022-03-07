using System.Text.Json;
using Amazon.SQS;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class QueueMessages : Controller
{
    //Todo: Add Deadletter queue example
    //Todo: refactor exception types
    
    private readonly ISQSService _sqsService;
    
    public QueueMessages(ISQSService sqsService) => _sqsService = sqsService;

    [HttpPost]
    public async Task<IActionResult> CreateQueueRequest(string queueName)
    {
        try
        {
            //TODO: Adding defense in case queue already exists
            var response = await _sqsService.CreateQueue(queueName);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetQueues()
    {
        var response = await _sqsService.GetQueues();
        var json = JsonSerializer.Serialize(response);
        return Ok(json);
    }

    [HttpPost]
    public async Task<IActionResult> PostMessage(string queueName, string message)
    {
        try
        {
            var response = await _sqsService.PostMessage(queueName, message);
            return Ok(response);
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
            var response = await _sqsService.GetMessage(queueName);
            return Ok(response);
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
            var response = await _sqsService.RemoveMessage(queueName, messageHandle);
            return Ok(response);
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
            var response = await _sqsService.DeleteQueue(queueName);
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}