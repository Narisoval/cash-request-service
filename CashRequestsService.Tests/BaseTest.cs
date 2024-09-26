using CashRequestsService.Tests.WebApplicationFactories;

namespace CashRequestsService.Tests;

[Collection("TestContainer Collection")]
public class BaseTest : IClassFixture<ApiWebApplicationFactory>, IClassFixture<BackendWebApplicationFactory>
{
    protected readonly HttpClient _client;

    protected BaseTest(ApiWebApplicationFactory apiFactory, BackendWebApplicationFactory backendFactory)
    {
        _client = apiFactory.CreateClient();
        _ = backendFactory.CreateClient();
    }
}