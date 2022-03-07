using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class Kinesis : Controller
{
    private readonly IKinesis _kinesis;
    public Kinesis(IConfiguration configuration, IKinesis kinesis)
    {
        _kinesis = kinesis;
    }

    [HttpGet]
    public async Task<IActionResult> CreateStream(string name)
    {
        var response = await _kinesis.CreateStream(name);
        if (response != HttpStatusCode.OK)
            return BadRequest($"Something went wrong while creating your stream {name}");

        return Ok($"Stream {name} has been created");
    }

    [HttpGet]
    public async Task<IActionResult> ListStreams()
    {
        var response = await _kinesis.ListStreams();
        if (response == null || !response.Any())
            return BadRequest("Something when wrong, while listing your streams or you have none");

        var output = JsonSerializer.Serialize(response);
        return Ok(output);
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveStream(string name)
    {
        //TODO: stop error when stream name doesn't exist
        var response = await _kinesis.RemoveStream(name);
        if (response != HttpStatusCode.OK)
            return BadRequest($"Unable to remove stream {name}");

        return Ok($"Stream {name} has been removed");
    }

    [HttpPost]
    public async Task<IActionResult> AddDataToStream(string name, string data)
    {
        var response = await _kinesis.AddDataToStream(name, data);
        
        return Ok($"{response}");
    }

    [HttpGet]
    public async Task<IActionResult> ReadFromStream(string name)
    {
        var output = await _kinesis.ReadFromStream(name);
        return Ok(JsonSerializer.Serialize(output));
    }
}