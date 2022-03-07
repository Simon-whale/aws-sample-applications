using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[Controller]/[Action]")]
public class DynamoDbController : Controller
{
    private readonly AmazonDynamoDBClient _client;

    public DynamoDbController(IConfiguration configuration)
    {
        _client = new(new AmazonDynamoDBConfig
        {
            ServiceURL = configuration["awsServer"]
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> ListDatabase()
    {
        //TODO fic this as it is broken
        var lastTableNameEvaluated = String.Empty;
        var output = new List<string>();
        
        do
        {
            try
            {
                var response = await _client.ListTablesAsync(new ListTablesRequest
                {
                    Limit = 2,
                    ExclusiveStartTableName = lastTableNameEvaluated
                });

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    return BadRequest();

                foreach (var name in response.TableNames)
                    output.Add(name);

                lastTableNameEvaluated = response.LastEvaluatedTableName;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        } while (lastTableNameEvaluated != null);

        return Ok(JsonSerializer.Serialize(output));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTable(string name)
    {
        var response = await _client.CreateTableAsync(new CreateTableRequest
        {
            TableName = name,
            AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "PersonId",
                    AttributeType = "N"
                }
            },
            KeySchema = new List<KeySchemaElement>()
            {
                new KeySchemaElement()
                {
                    AttributeName = "PersonId",
                    KeyType = "HASH"
                }
            },
            ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 5,
                WriteCapacityUnits = 2
            }
        });

        var result = response.TableDescription;
        var status = result.TableStatus;
        return Ok(status);
    }

    [HttpGet]
    public async Task<string> GetTable()
    {
        var response = await _client.li
    }
}