using Amazon.DynamoDBv2.DataModel;
namespace TelemetryAdmin.Models
{
    [DynamoDBTable("VehicleTelemetryData")]
    public class VehicleTelemetryData
    {
        [DynamoDBHashKey]
        public long VehicleId { get; init; }

        [DynamoDBRangeKey]
        public DateTime TimeStamp { get; init; }

        public string Location { get; init; } = default!;

        public int Speed { get; init; }

        public decimal FuelLevel { get; init; }

        public int EngineTemperature { get; init; }

        public int? CargoTemperature { get; init; }

        public bool IgnitionStatus { get; init; }
    }
}
