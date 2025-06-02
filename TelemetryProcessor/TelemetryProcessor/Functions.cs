using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;
using TelemetryProcessor.Models;
using static Amazon.Lambda.SQSEvents.SQSBatchResponse;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TelemetryProcessor;

public class Functions
{
    private JsonSerializerOptions OPTIONS = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway 
    /// </summary>
    /// <remarks>
    /// This uses the <see href="https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.Annotations/README.md">Lambda Annotations</see> 
    /// programming model to bridge the gap between the Lambda programming model and a more idiomatic .NET model.
    /// 
    /// This automatically handles reading parameters from an APIGatewayProxyRequest
    /// as well as syncing the function definitions to serverless.template each time you build.
    /// 
    /// If you do not wish to use this model and need to manipulate the API Gateway 
    /// objects directly, see the accompanying Readme.md for instructions.
    /// </remarks>
    /// <param name="context">Information about the invocation, function, and execution environment</param>
    /// <returns>The response as an implicit <see cref="APIGatewayProxyResponse"/></returns>
    [LambdaFunction]
    public async Task FunctionHandler([FromServices] IAmazonDynamoDB dynamoDbClient, SQSEvent evnt, ILambdaContext context)
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

    private async Task ProcessMessageAsync(IAmazonDynamoDB dynamoDbClient, SQSEvent.SQSMessage message, ILambdaContext context)
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
    }
}
