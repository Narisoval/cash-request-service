using System.Reflection;
using CashRequestService.Backend.Extensions;
using CashRequestService.Backend.Services.UnitOfWork;
using CashRequestService.Backend.Settings;
using MassTransit;
using Serilog;

namespace CashRequestService.Backend;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(_configuration["MessageBroker:Host"]), h =>
                {
                    h.Username(_configuration["MessageBroker:UserName"]);
                    h.Password(_configuration["MessageBroker:Password"]);
                });

                configurator.ConfigureEndpoints(context);
            });

            x.AddConsumers(Assembly.GetExecutingAssembly());
        });

        services.Configure<RepositorySettings>(_configuration.GetSection("RepositorySettings"));
        services.Configure<CashRequestSettings>(_configuration.GetSection("CashRequestSettings"));

        services.RegisterSingletonProvider(
            typeof(IUnitOfWork), 
            _configuration, 
            "UnitOfWork", 
            null, 
            typeof(PgSqlUnitOfWork), 
            typeof(MsSqlUnitOfWork));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
    }
    
}
