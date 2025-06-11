using Amazon.DynamoDBv2.DataModel;
namespace TelemetryAdmin.Models
{
    [DynamoDBTable("TelemetryAlertsData")]
    public class VehicleTelemetryAlertsData
    {
        [DynamoDBHashKey(AttributeName = "vehicleId")]
        public long VehicleId { get; init; }

        [DynamoDBRangeKey(AttributeName = "timestamp")]
        public DateTime Timestamp { get; init; }

        public string Location { get; init; } = default!;

        public int Speed { get; init; }

        public decimal FuelLevel { get; init; }

        public int EngineTemperature { get; init; }

        public int? CargoTemperature { get; init; }

        public bool IgnitionStatus { get; init; }
    }
}
