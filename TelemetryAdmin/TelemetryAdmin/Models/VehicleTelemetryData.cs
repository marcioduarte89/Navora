using Amazon.DynamoDBv2.DataModel;
namespace TelemetryAdmin.Models
{
    [DynamoDBTable("TelemetryData")]
    public class VehicleTelemetryData
    {
        [DynamoDBHashKey(AttributeName = "vehicleId")]
        public long VehicleId { get; init; }

        [DynamoDBRangeKey(AttributeName = "timestamp")]
        public DateTime Timestamp { get; init; }

        [DynamoDBProperty(AttributeName = "location")]
        public string Location { get; init; } = default!;

        [DynamoDBProperty(AttributeName = "speed")]
        public int Speed { get; init; }

        [DynamoDBProperty(AttributeName = "fuelLevel")]
        public decimal FuelLevel { get; init; }

        [DynamoDBProperty(AttributeName = "engineTemperature")]
        public int EngineTemperature { get; init; }

        [DynamoDBProperty(AttributeName = "cargoTemperature")]
        public int? CargoTemperature { get; init; }

        [DynamoDBProperty(AttributeName = "ignitionStatus")]
        public bool IgnitionStatus { get; init; }
    }
}
