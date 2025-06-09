using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nest;
using System.Net;
using System.Text.Json;
using TelemetryAdmin.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TelemetryAdmin;

public class Function
{
    private readonly IDynamoDBContext _dynamoDbContext;

    public Function(IDynamoDBContext dynamoDbClient)
    {
        _dynamoDbContext = dynamoDbClient;
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/vehicle-telemetry/live")]
    public async Task<IEnumerable<VehicleTelemetryData>> GetVehiclesTelemetryLiveData()
    {
        // DON'T DO THIS - THIS IS VERY VERY BAD
        // THIS WILL QUERY THE ENTIRE DYNAMO DB DATABASE
        // THIS OPERATION SHOULDN'T BE DONE ON THE MAIN TABLE - JUST FOR TESTING PURPOSES
        var telemetry = await Scan();
        return telemetry
            .GroupBy(x => x.VehicleId)
            .Select(g => g.OrderByDescending(t => t.TimeStamp).First())
            .ToList();
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/vehicle-telemetry/{vehicleId}/history")]
    public async Task<List<VehicleTelemetryData>> GetVehicleTelemetryDataHistory(long vehicleId)
    {
        var vehicleTelemetryData = await Query(vehicleId);
        return vehicleTelemetryData;
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/vehicle-telemetry/alerts")]
    public APIGatewayProxyResponse GetVehiclesTelemetryAlertData()
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/vehicle-telemetry/alerts/{vehicleId}")]
    public APIGatewayProxyResponse GetVehicleTelemetryAlertData(long vehicleId)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    private async Task<List<VehicleTelemetryData>> Query(long vehicleId)
    {
        var telemetryData = new List<VehicleTelemetryData>();
        var queryResults = _dynamoDbContext.QueryAsync<VehicleTelemetryData>(vehicleId.ToString());
        while (!queryResults.IsDone)
        {
            telemetryData.AddRange(await queryResults.GetNextSetAsync());

        }
            return telemetryData;
    }

    private async Task<IEnumerable<VehicleTelemetryData>> Scan()
    {
        // this is quite bad, don't scan. Just for testing purposes
        var telemetryList = new List<VehicleTelemetryData>();
        var scanConditons = Enumerable.Empty<ScanCondition>();
        var scanResults = _dynamoDbContext.ScanAsync<VehicleTelemetryData>(scanConditons);
        while (!scanResults.IsDone)
        {
            telemetryList.AddRange(await scanResults.GetNextSetAsync());
        }
        
        return telemetryList;
    }
}
