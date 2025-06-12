using GeoCoordinatePortable;

namespace AlertsProcessor.Models
{
    public class TelemetryAlertData
    {
        public long VehicleId { get; init; }

        public DateTime Timestamp { get; init; }

        public GeoCoordinate Location { get; init; } = default!;

        public int Speed { get; init; }

        public decimal FuelLevel { get; init; }

        public int EngineTemperature { get; init; }

        public int? CargoTemperature { get; init; }

        public bool IgnitionStatus { get; init; }        
    }
}
