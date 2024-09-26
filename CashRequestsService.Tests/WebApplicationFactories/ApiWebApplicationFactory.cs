using CashRequestService.Api.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MassTransit;

namespace CashRequestsService.Tests.WebApplicationFactories;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestContainerSetup _testContainerSetup;

    public ApiWebApplicationFactory(TestContainerSetup testContainerSetup)
    {
        _testContainerSetup = testContainerSetup;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var newSettings = new Dictionary<string, string>
            {
                ["MessageBroker:Host"] = _testContainerSetup.RabbitMqContainer.GetConnectionString(),
                ["MessageBroker:Username"] = "guest",
                ["MessageBroker:Password"] = "guest"
            };
            config.AddInMemoryCollection(newSettings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveMassTransitHostedService();
            
            var descriptors = services.Where(d => 
                    d.ServiceType.Namespace.Contains("MassTransit",StringComparison.OrdinalIgnoreCase)).ToList();
            
            foreach (var d in descriptors) 
            {
                services.Remove(d);
            }     

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, configurator) =>
                {
                    var settings = context.GetRequiredService<IOptions<MessageBrokerSettings>>().Value;
                    configurator.Host(new Uri(settings.Host), h =>
                    {
                        h.Username(settings.Username);
                        h.Password(settings.Password);
                    });
                });
            });
        });
    }
}
