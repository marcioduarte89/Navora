using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;
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
    public async Task<IEnumerable<VehicleTelemetryData>> GetVehiclesLiveTelemetry()
    {
        // DON'T DO THIS - THIS IS VERY VERY BAD
        // THIS WILL QUERY THE ENTIRE DYNAMO DB DATABASE
        // THIS OPERATION SHOULDN'T BE DONE ON THE MAIN TABLE - JUST FOR TESTING PURPOSES
        var telemetry = await Scan<VehicleTelemetryData>();
        return telemetry
            .GroupBy(x => x.VehicleId)
            .Select(g => g.OrderByDescending(t => t.Timestamp).First())
            .ToList();
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/vehicle-telemetry/{vehicleId}/history")]
    public async Task<List<VehicleTelemetryData>> GetVehicleTelemetryHistory(long vehicleId)
    {
        return await Query<VehicleTelemetryData>(vehicleId);
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/vehicle-telemetry/alerts")]
    public async Task<List<VehicleTelemetryAlertsData>> GetVehiclesAlertTelemetry()
    {
        // DON'T DO THIS - THIS IS VERY VERY BAD
        // THIS WILL QUERY THE ENTIRE DYNAMO DB DATABASE
        // THIS OPERATION SHOULDN'T BE DONE ON THE MAIN TABLE - JUST FOR TESTING PURPOSES
        var telemetry = await Scan<VehicleTelemetryAlertsData>();
        return telemetry
            .GroupBy(x => x.VehicleId)
            .Select(g => g.OrderByDescending(t => t.Timestamp).First())
            .ToList();
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/vehicle-telemetry/alerts/{vehicleId}")]
    public async Task<List<VehicleTelemetryAlertsData>> GetVehicleAlertTelemetry(long vehicleId)
    {
        return await Query<VehicleTelemetryAlertsData>(vehicleId);
    }

    private async Task<List<T>> Query<T>(long vehicleId)
    {
        var telemetryData = new List<T>();
        var queryResults = _dynamoDbContext.QueryAsync<T>(vehicleId);
        while (!queryResults.IsDone)
        {
            telemetryData.AddRange(await queryResults.GetNextSetAsync());

        }
        
        return telemetryData;
    }

    private async Task<IEnumerable<T>> Scan<T>()
    {
        // this is quite bad, don't scan. Just for testing purposes
        var telemetryList = new List<T>();
        var scanConditons = Enumerable.Empty<ScanCondition>();
        var scanResults = _dynamoDbContext.ScanAsync<T>(scanConditons);
        while (!scanResults.IsDone)
        {
            telemetryList.AddRange(await scanResults.GetNextSetAsync());
        }
        
        return telemetryList;
    }
}