using System.Net;
using System.Text;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using WebApplication1.Interfaces;

namespace WebApplication1.Services;

public class KinesisService : IKinesis
{
    private readonly AmazonKinesisClient _client;

    public KinesisService(IConfiguration configuration)
    {
        _client = new AmazonKinesisClient(new AmazonKinesisConfig
        {
            ServiceURL = configuration["awsServer"]
        });
    }

    public async Task<HttpStatusCode> CreateStream(string name)
    {
        var response = await _client.CreateStreamAsync(new CreateStreamRequest
        {
            ShardCount = 1,
            StreamName = name
        });

        return response.HttpStatusCode;
    }

    public async Task<List<string>> ListStreams()
    {
        var response = await _client.ListStreamsAsync();
        return response.StreamNames;
    }

    public async Task<HttpStatusCode> RemoveStream(string name)
    {
        //TODO: stop error when stream name doesn't exist
        var response = await _client.DeleteStreamAsync(new DeleteStreamRequest
        {
            StreamName = name,
            EnforceConsumerDeletion = true
        });
        
        return response.HttpStatusCode;
    }

    public async Task<string> AddDataToStream(string name, string data)
    {
        var response = await _client.PutRecordAsync(new PutRecordRequest
        {
            StreamName = name,
            Data = new MemoryStream(Encoding.UTF8.GetBytes(data)),
            PartitionKey = $"key{DateTime.Now.Ticks.ToString()}"
        });

        return response.ShardId;
    }

    public async Task<List<string>> ReadFromStream(string name)
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

        return output;
    }
}