using CashRequestService.Api.ApiModels;
using CashRequestService.Api.Middleware;
using CashRequestService.Api.Settings;
using CashRequestService.Api.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) 
    => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddScoped<IValidator<CashRequestCreationRequest>, CashRequestValidator>();
builder.Services.AddScoped<IValidator<CashRequestQueryModel>, CashRequestQueryModelValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("MessageBroker"));

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    MessageBrokerSettings messageBrokerSettings = builder.Configuration.GetSection("MessageBroker").Get<MessageBrokerSettings>();
 
    x.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(messageBrokerSettings.Host), h =>
        {
            h.Username(messageBrokerSettings.Username);
            h.Password(messageBrokerSettings.Password);
        });
        
        configurator.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<RequestLogContextMiddleware>();
    
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Warning;
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();

public partial class Program { } //FOR TESTS
