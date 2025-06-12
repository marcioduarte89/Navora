using IoTDeviceApp;
using IoTDeviceApp.Models;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

var config = new ConfigurationBuilder()
     .AddJsonFile($"appsettings.json")
     .Build();

var iotSettings = config.GetSection("iotSettings").Get<IoTSettings>();

var caCert = X509Certificate.CreateFromCertFile($"{AppContext.BaseDirectory}\\certs\\AmazonRootCA1.pem");
var clientCert = new X509Certificate2($"{AppContext.BaseDirectory}\\certs\\certificate.cert.pfx", "Qwerty12345");

var caChain = new X509Certificate2Collection();
caChain.ImportFromPem(caCert.GetCertHashString());

var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithClientId(Guid.NewGuid().ToString())
    .WithTcpServer(iotSettings.Endpoint, iotSettings.Port)
    .WithTlsOptions(
        new MqttClientTlsOptionsBuilder()
        .WithTrustChain(caChain)
        .WithClientCertificates(new List<X509Certificate2> { clientCert })
        .WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12)
        .Build())
    .WithCleanStart(false) // persistent session
    .WithProtocolVersion(MqttProtocolVersion.V500)
    .Build();

var mqttFactory = new MqttClientFactory();
var mqttClient = mqttFactory.CreateMqttClient();

var connection = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

for (int i = 0; i < 10; i++)
{
    for (int j = 1; j <= 5; j++)
    {
        var vehicleTelemetryData = default(VehicleTelemetryData);
        if (j >= 4)
        {
            vehicleTelemetryData = new VehicleTelemetryData()
            {
                VehicleId = j,
                TimeStamp = DateTime.UtcNow,
                Speed = 100,
                CargoTemperature = 50,
                EngineTemperature = 50,
                FuelLevel = 90,
                IgnitionStatus = true,
                Location = new Nest.GeoCoordinate(10, 10)
            };
        }
        else
        {
            vehicleTelemetryData = new VehicleTelemetryData()
            {
                VehicleId = j,
                TimeStamp = DateTime.UtcNow,
                Speed = 60,
                CargoTemperature = 50,
                EngineTemperature = 50,
                FuelLevel = 90,
                IgnitionStatus = true,
                Location = new Nest.GeoCoordinate(10, 10)
            };
        }

        var serializedVehicle = JsonSerializer.Serialize(vehicleTelemetryData);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"{iotSettings.Topic}/{iotSettings.VehicleId}")
            .WithPayload(serializedVehicle)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        Console.WriteLine($"Publishing data for vehicle id {j} with data {serializedVehicle}");

        var result = await mqttClient.PublishAsync(message, CancellationToken.None);

        Console.WriteLine($"Vehicle id {j} success status: {result.IsSuccess}");
    }

    await Task.Delay(10000);
}
Console.ReadKey();

await mqttClient.DisconnectAsync();