using GeoCoordinatePortable;

namespace TelemetryProcessor.Models
{
    public class VehicleTelemetryData
    {
        public long VehicleId { get; init; }

        public DateTime Timestamp { get; init; }

        public GeoCoordinate Location { get; init; } = default!;

        public int Speed { get; init; }

        public decimal FuelLevel { get; init; }

        public int EngineTemperature { get; init; }

        public int? CargoTemperature { get; init; }

        public bool IgnitionStatus { get; init; }

        /// <summary>
        /// Having some dummy value ranges here
        /// </summary>
        /// <returns></returns>
        public bool HasAlert()
        {
            return Speed > 90 || FuelLevel < 10 || EngineTemperature > 90 || CargoTemperature > 50;
        }
    }
}
