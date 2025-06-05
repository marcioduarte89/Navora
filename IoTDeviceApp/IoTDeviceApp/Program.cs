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

while (true)
{
    var vehicleTelemetryData = new VehicleTelemetryData()
    {
        VehicleId = iotSettings.VehicleId,
        TimeStamp = DateTime.UtcNow,
        Speed = 100,
        CargoTemperature = 50,
        EngineTemperature = 50,
        FuelLevel = 90,
        IgnitionStatus = true,
        Location = new Nest.GeoCoordinate(10, 10)
    };

    var message = new MqttApplicationMessageBuilder()
        .WithTopic($"{iotSettings.Topic}/{iotSettings.VehicleId}")
        .WithPayload(JsonSerializer.Serialize(vehicleTelemetryData))
        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
        .Build();

    var result = await mqttClient.PublishAsync(message, CancellationToken.None);

    await Task.Delay(10000);
}

// fix this!
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

await mqttClient.DisconnectAsync();