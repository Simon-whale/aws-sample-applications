using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class Kinesis : Controller
{
    private readonly AmazonKinesisClient _client;
    
    public Kinesis(IConfiguration configuration)
    {
        _client = new AmazonKinesisClient(new AmazonKinesisConfig
        {
            ServiceURL = configuration["awsServer"]
        });
    }

    [HttpGet]
    public async Task<IActionResult> CreateStream(string name)
    {
        var response = await _client.CreateStreamAsync(new CreateStreamRequest
        {
            ShardCount = 1,
            StreamName = name
        });

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return BadRequest($"Something went wrong while creating your stream {name}");

        return Ok($"Stream {name} has been created");
    }

    [HttpGet]
    public async Task<IActionResult> ListStreams()
    {
        var response = await _client.ListStreamsAsync();

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return BadRequest("Something when wrong, while listing your streams");

        var output = JsonSerializer.Serialize(response.StreamNames);
        return Ok(output);
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveStream(string name)
    {
        //TODO: stop error when stream name doesn't exist
        var response = await _client.DeleteStreamAsync(new DeleteStreamRequest
        {
            StreamName = name,
            EnforceConsumerDeletion = true
        });

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return BadRequest($"Unable to remove stream {name}");

        return Ok($"Stream {name} has been removed");
    }

    [HttpPost]
    public async Task<IActionResult> AddDataToStream(string name, string data)
    {
        var response = await _client.PutRecordAsync(new PutRecordRequest
        {
            StreamName = name,
            Data = new MemoryStream(Encoding.UTF8.GetBytes(data)),
            PartitionKey = $"key{DateTime.Now.Ticks.ToString()}"
        });

        if (response.HttpStatusCode != HttpStatusCode.OK)
            return BadRequest($"Something went wrong {response.ResponseMetadata.ChecksumValidationStatus}");

        return Ok($"{response.ShardId}");
    }

    [HttpGet]
    public async Task<IActionResult> ReadFromStream(string name)
    {
        var output = new List<string>();
        
        var describeStreamResponse = await _client.DescribeStreamAsync(new DescribeStreamRequest
        {
            StreamName = name
        });
        var shards = describeStreamResponse.StreamDescription.Shards;
        foreach (var shard in shards)
        {
            var getShardIteratorResponse = await _client.GetShardIteratorAsync(new GetShardIteratorRequest
            {
                StreamName = name,
                ShardId = shard.ShardId,
                ShardIteratorType = ShardIteratorType.TRIM_HORIZON
            });
            var shardIterator = getShardIteratorResponse.ShardIterator;
            while (!string.IsNullOrEmpty(shardIterator))
            {
                var getRecordsResponse = await _client.GetRecordsAsync(new GetRecordsRequest
                {
                    Limit = 100,
                    ShardIterator = shardIterator
                });
                var nextIterator = getRecordsResponse.NextShardIterator;
                var records = getRecordsResponse.Records;

                if (records.Count > 0)
                {
                    foreach (var record in records)
                    {
                        var streamData = Encoding.UTF8.GetString(record.Data.ToArray());
                        Console.WriteLine(streamData);
                        output.Add(streamData);
                    }

                    //believe there is a better way of this
                    break;
                }
            }
        }

        return Ok(JsonSerializer.Serialize(output));
    }
}