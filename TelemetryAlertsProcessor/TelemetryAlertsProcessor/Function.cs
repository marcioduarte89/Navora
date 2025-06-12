using AlertsProcessor.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using System.Text.Json;
using static Amazon.Lambda.SNSEvents.SNSEvent;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AlertsProcessor;

public class Function
{
    private JsonSerializerOptions OPTIONS = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    [LambdaFunction]
    public async Task FunctionHandler(
        [FromServices] IAmazonDynamoDB dynamoDbClient, 
        SNSEvent evnt,
        ILambdaContext context)
    {
        foreach(var record in evnt.Records)
        {
            await ProcessRecordAsync(dynamoDbClient, record, context);
        }
    }

    private async Task ProcessRecordAsync(
        IAmazonDynamoDB dynamoDbClient,
        SNSRecord record,
        ILambdaContext context)
    {
        Console.WriteLine($"Processing record {record.Sns.Message}");

        var payload = JsonSerializer.Deserialize<TelemetryAlertData>(record.Sns.Message, OPTIONS);

        var item = new Dictionary<string, AttributeValue>
        {
            { "vehicleId", new AttributeValue { N = payload.VehicleId.ToString() } },
            { "timestamp", new AttributeValue { S = payload.Timestamp.ToString("o") } },
            { "location", new AttributeValue { S = payload.Location.ToString() } },
            { "speed", new AttributeValue { S = payload.Speed.ToString() } },
            { "fuelLevel", new AttributeValue { S = payload.FuelLevel.ToString() } },
            { "engineTemperature", new AttributeValue { S = payload.EngineTemperature.ToString() } },
            { "cargoTemperature", new AttributeValue { S = payload.CargoTemperature.ToString() } },
            { "ignitionStatus", new AttributeValue { S = payload.IgnitionStatus.ToString() } },
            { "messageId", new AttributeValue { S = record.Sns.MessageId.ToString() } },
        };

        var request = new PutItemRequest
        {
            TableName = "TelemetryAlertsData",
            Item = item,
            ConditionExpression = "attribute_not_exists(MessageId)"
        };

        var response = await dynamoDbClient.PutItemAsync(request);

        Console.WriteLine($"Putting item in table {response.HttpStatusCode}");

        // TODO: Do interesting work based on the new message
        await Task.CompletedTask;
    }
}