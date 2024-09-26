using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CashRequestService.Backend;
using CashRequestService.Backend.Services.UnitOfWork;
using DotNet.Testcontainers.Containers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;


namespace CashRequestsService.Tests.WebApplicationFactories;

public class BackendWebApplicationFactory : WebApplicationFactory<Startup>
{
    private readonly TestContainerSetup _testContainerSetup;
    private IContainer _dbContainer;
    private string _dbConnectionString;
    private IConfiguration _testConfiguration;

    public BackendWebApplicationFactory(TestContainerSetup testContainerSetup)
    {
        _testContainerSetup = testContainerSetup;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var test = Directory.GetParent(AppContext.BaseDirectory).FullName;
        
        _testConfiguration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
            .AddJsonFile("test-appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        if (!Enum.TryParse<TestDbType>(_testConfiguration["DbType"], true, out TestDbType dbType))
        {
            throw new InvalidOperationException($"Invalid DbType value in appsettings.json: {_testConfiguration["DbType"]}");
        }

        Console.WriteLine($"DbType from appsettings.json: {dbType}");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            switch (dbType)
            {
                case TestDbType.PostgreSQL:
                {
                    PostgreSqlContainer concreteDbContainer = new PostgreSqlBuilder()
                        .WithImage("postgres:15.8-alpine")
                        .WithDatabase("testdb")
                        .WithUsername("postgres")
                        .WithPassword("yourpassword")
                        .Build();

                    concreteDbContainer.StartAsync().GetAwaiter().GetResult();
                    
                    _dbConnectionString = concreteDbContainer.GetConnectionString();
                    _dbContainer = concreteDbContainer;

                    break;
                }
                case TestDbType.MSSQL:
                {
                    var concreteDbContainer = new MsSqlBuilder()
                        .WithEnvironment(new Dictionary<string, string>(){{ "ACCEPT_EULA","Y"}})
                        .WithPassword("StrongP@ssw0rd")
                        .Build();

                    concreteDbContainer.StartAsync().GetAwaiter().GetResult();
                    
                    _dbConnectionString = concreteDbContainer.GetConnectionString();
                    _dbContainer = concreteDbContainer;

                    break;
                }
            }

            ExecuteSqlScript(dbType);
            
            var newSettings = new Dictionary<string, string>
            {
                ["RepositorySettings:ConnectionString"] = _dbConnectionString
            };
            config.AddInMemoryCollection(newSettings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveMassTransitHostedService();

            var descriptors = services.Where(d =>
                d.ServiceType.Namespace.Contains("MassTransit", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(new Uri(_testContainerSetup.RabbitMqContainer.GetConnectionString()), h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    configurator.ConfigureEndpoints(context);
                });

                x.AddConsumers(Assembly.GetAssembly(typeof(Startup)));
            });

            
            ServiceDescriptor unitOfWOrkDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUnitOfWork));

            if (unitOfWOrkDescriptor != null)
            {
                services.Remove(unitOfWOrkDescriptor);
            }

            if (dbType == TestDbType.PostgreSQL)
            {
                services.AddSingleton<IUnitOfWork, PgSqlUnitOfWork>();
            }
            else if (dbType == TestDbType.MSSQL)
            {
                services.AddSingleton<IUnitOfWork, MsSqlUnitOfWork>();
            }
        });
    }

    public override async ValueTask DisposeAsync()
    {
        if (_dbContainer != null)
        {
            await _dbContainer.StopAsync();
        }
    }

    private void ExecuteSqlScript(TestDbType dbType)
    {
        string scriptPath = dbType switch
        {
            TestDbType.PostgreSQL => _testConfiguration["DbInitialization:PostgreSQLScript"],
            TestDbType.MSSQL => _testConfiguration["DbInitialization:MSSQLScript"],
            _ => throw new InvalidOperationException($"Unsupported DbType: {dbType}")
        };

        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"Initialization script not found at: {scriptPath}");
        }

        string script = File.ReadAllText(scriptPath);

        switch (dbType)
        {
            case TestDbType.PostgreSQL:
            {
                using var connection = new NpgsqlConnection(_dbConnectionString);
                connection.Open();
                using var cmd = new NpgsqlCommand(script, connection);
                cmd.ExecuteNonQuery();
                break;
            }
            case TestDbType.MSSQL:
            {
                string[] sqlBatches = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                using var connection = new SqlConnection(_dbConnectionString);
                connection.Open();

                foreach (var batch in sqlBatches)
                {
                    string sqlBatch = batch.Trim();

                    if (string.IsNullOrWhiteSpace(sqlBatch))
                    {
                        continue;
                    }

                    using var cmd = new SqlCommand(sqlBatch, connection);
    
                    cmd.ExecuteNonQuery();
                }
                
                break;
            }
        }
    }
    
}
