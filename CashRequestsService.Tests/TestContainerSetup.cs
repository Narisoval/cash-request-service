using Testcontainers.RabbitMq;

public class TestContainerSetup : IAsyncLifetime
{
    public RabbitMqContainer RabbitMqContainer { get; private set; }

    public async Task InitializeAsync()
    {
        RabbitMqContainer = new RabbitMqBuilder()
            .WithUsername("guest")
            .WithPassword("guest")
            .Build();
        await RabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await RabbitMqContainer.StopAsync();
    }
}