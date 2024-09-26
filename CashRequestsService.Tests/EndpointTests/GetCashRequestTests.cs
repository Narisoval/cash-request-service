using System.Net;
using CashRequestsService.Tests.Models;
using CashRequestsService.Tests.WebApplicationFactories;
using FluentAssertions;
using Newtonsoft.Json;

namespace CashRequestsService.Tests.EndpointTests;

public class GetCashRequestTests : BaseTest
{
    public GetCashRequestTests(ApiWebApplicationFactory apiFactory, 
        BackendWebApplicationFactory backendFactory) : 
        base(apiFactory, backendFactory)
    {
        CashRequestInitializer.InitializeAsync(_client).GetAwaiter().GetResult();
    }
    
    private const string ValidClientId1 = "client1";
    private const string ValidDepartmentAddress1 = "123 Main St";
    
    #region Query Validation Tests

    [Fact]
    public async Task GetCashRequests_ShouldReturnBadRequest_WhenNoParametersAreProvided()
    {
        // Act
        var response = await GetCashRequestsAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCashRequests_ShouldReturnBadRequest_WhenRequestIdIsZero()
    {
        // Act
        var response = await GetCashRequestsAsync(requestId: "0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task GetCashRequests_ShouldReturnBadRequest_WhenRequestIdIsNegative()
    {
        // Act
        var response = await GetCashRequestsAsync(requestId: "-10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCashRequests_ShouldReturnBadRequest_WhenOnlyClientIdIsProvided()
    {
        // Act
        var response = await GetCashRequestsAsync(clientId: ValidClientId1);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCashRequests_ShouldReturnBadRequest_WhenOnlyDepartmentAddressIsProvided()
    {
        // Act
        var response = await GetCashRequestsAsync(departmentAddress: ValidDepartmentAddress1);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region 404 Not Found Tests

    [Fact]
    public async Task GetCashRequests_ShouldReturnNotFound_WhenNonExistingRequestIdIsProvided()
    {
        // Arrange
        const string nonExistingId = "9999";

        // Act
        var response = await GetCashRequestsAsync(requestId: nonExistingId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCashRequests_ShouldReturnNotFound_WhenNonExistingClientIdAndDepartmentAreProvided()
    {
        // Act
        var response = await GetCashRequestsAsync(clientId: "nonExistingClient", departmentAddress: "nonExistingDept");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Correct Data Retrieval Tests

   
    [Fact]
    public async Task GetCashRequests_ShouldReturnCashRequests_WhenValidRequestIdIsProvided()
    {
        // Act
        var response = await GetCashRequestsAsync(requestId: "1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
    
        var cashRequests = JsonConvert.DeserializeObject<List<CashRequestResponse>>(responseBody);

        cashRequests.Should().NotBeNull();
        cashRequests.Should().HaveCount(1);

        var cashRequest = cashRequests.First();
        cashRequest.Amount.Should().BeGreaterThan(0);
        cashRequest.Currency.Should().NotBeNullOrEmpty();
        cashRequest.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCashRequests_ShouldReturnCashRequests_WhenValidClientIdAndDepartmentAreProvided()
    {
        // Act
        var response = await GetCashRequestsAsync(clientId: ValidClientId1, departmentAddress: ValidDepartmentAddress1);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseBody = await response.Content.ReadAsStringAsync();
        var cashRequests = JsonConvert.DeserializeObject<List<CashRequestResponse>>(responseBody);

        cashRequests.Should().NotBeNull();
        cashRequests.Should().HaveCount(2);

        foreach (var cashRequest in cashRequests)
        {
            cashRequest.Amount.Should().BeGreaterThan(0);
            cashRequest.Currency.Should().NotBeNullOrEmpty();
            cashRequest.Status.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    private async Task<HttpResponseMessage> GetCashRequestsAsync(string requestId = null, string clientId = null, string departmentAddress = null)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(requestId))
        {
            queryParams.Add($"request_id={WebUtility.UrlEncode(requestId)}");
        }

        if (!string.IsNullOrEmpty(clientId))
        {
            queryParams.Add($"client_id={WebUtility.UrlEncode(clientId)}");
        }

        if (!string.IsNullOrEmpty(departmentAddress))
        {
            queryParams.Add($"department_address={WebUtility.UrlEncode(departmentAddress)}");
        }

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;        
        
        return await _client.GetAsync($"http://localhost:5000/api/cashrequest{queryString}");
    }

}