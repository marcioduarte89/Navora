using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Net;
using System.Text.Json;
using TelemetryProcessor.Models;
using TelemetryProcessor.Services;
using static Amazon.Lambda.SQSEvents.SQSBatchResponse;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TelemetryProcessor;

public class Functions
{
    private readonly ITopicsService _topicsService;
    private readonly IAmazonSimpleNotificationService _snsService;
    private JsonSerializerOptions OPTIONS = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    public Functions(ITopicsService topicsService, IAmazonSimpleNotificationService snsService)
    {
        _topicsService = topicsService;
        _snsService = snsService;
    }

    [LambdaFunction]
    public async Task FunctionHandler(
        [FromServices] IAmazonDynamoDB dynamoDbClient,
        SQSEvent evnt,
        ILambdaContext context)
    {
        List<BatchItemFailure> batchItemFailures = [];

        foreach (var message in evnt.Records)
        {
            try
            {
                await ProcessMessageAsync(dynamoDbClient, message, context);
            }
            catch (Exception)
            {
                //Add failed message identifier to the batchItemFailures list
                batchItemFailures.Add(new BatchItemFailure { ItemIdentifier = message.MessageId });
            }
        }
    }

    private async Task ProcessMessageAsync(
        IAmazonDynamoDB dynamoDbClient,
        SQSEvent.SQSMessage message,
        ILambdaContext context)
    {
        var payload = JsonSerializer.Deserialize<VehicleTelemetryData>(message.Body, OPTIONS);

        var item = new Dictionary<string, AttributeValue>
        {
            { "vehicleId", new AttributeValue { S = payload.VehicleId.ToString() } },
            { "timestamp", new AttributeValue { S = payload.TimeStamp.ToString("o") } },
            { "location", new AttributeValue { S = payload.Location.ToString() } },
            { "speed", new AttributeValue { S = payload.Speed.ToString() } },
            { "fuelLevel", new AttributeValue { S = payload.FuelLevel.ToString() } },
            { "engineTemperature", new AttributeValue { S = payload.EngineTemperature.ToString() } },
            { "cargoTemperature", new AttributeValue { S = payload.CargoTemperature.ToString() } },
            { "ignitionStatus", new AttributeValue { S = payload.IgnitionStatus.ToString() } },
            { "messageId", new AttributeValue { S = message.MessageId.ToString() } },
        };

        var request = new PutItemRequest
        {
            TableName = "TelemetryData",
            Item = item,
            ConditionExpression = "attribute_not_exists(MessageId)"
        };

        var response = await dynamoDbClient.PutItemAsync(request);

        Console.WriteLine($"Pushing item to dynamodb status: {response.HttpStatusCode}");
        Console.WriteLine($"Has alert: {payload.HasAlert()}");

        if (response.HttpStatusCode == HttpStatusCode.OK && payload.HasAlert())
        {
            var topicArn = await _topicsService.GetTopicArnByName("AlertsTopic");

            Console.WriteLine($"Topic: {topicArn}");

            if (topicArn is null)
            {
                // handle it here (move message to error queue, etc)
            }

            var publishRequest = new PublishRequest
            {
                TopicArn = topicArn,
                Message = message.Body,
            };

            var snsResponse = await _snsService.PublishAsync(publishRequest);

            Console.WriteLine($"Successfully published message ID: {snsResponse.MessageId}");
        }
    }
}