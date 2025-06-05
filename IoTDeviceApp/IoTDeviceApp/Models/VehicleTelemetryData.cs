using Nest;

namespace IoTDeviceApp.Models
{
    public class VehicleTelemetryData
    {
        public long VehicleId { get; init; }

        public DateTime TimeStamp { get; init; }

        public GeoCoordinate Location { get; init; } = default!;

        public int Speed { get; init; }

        public decimal FuelLevel { get; init; }

        public int EngineTemperature { get; init; }

        public int? CargoTemperature { get; init; }

        public bool IgnitionStatus { get; init; }
    }
}
