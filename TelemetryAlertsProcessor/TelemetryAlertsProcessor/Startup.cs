using Amazon.DynamoDBv2;
using Amazon.Lambda.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlertsProcessor
{
    [LambdaStartup]

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //// Example of creating the IConfiguration object and
            //// adding it to the dependency injection container.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true);

            //// Add AWS Systems Manager as a potential provider for the configuration. This is 
            //// available with the Amazon.Extensions.Configuration.SystemsManager NuGet package.
            //builder.AddSystemsManager("/app/settings");

            var configuration = builder.Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonDynamoDB>();
        }
    }
}
