using System.Net;
using System.Text;
using CashRequestsService.Tests.WebApplicationFactories;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CashRequestsService.Tests.EndpointTests;

public class CreateCashRequestTests : BaseTest
{
    public CreateCashRequestTests(
        ApiWebApplicationFactory apiFactory, 
        BackendWebApplicationFactory backendFactory) : base( apiFactory, backendFactory)
    {

    }
    
    private const string ValidClientId = "1b3ea56b-5746-46fc-9c07-d73ca4f9d47f";
    private const string ValidDepartmentAddress = "123 Test St";
    private const decimal ValidAmount = 5000;
    private const string ValidCurrency = "USD";

    #region Model validation tests

    [Fact]
    public async Task CreateCashRequest_ShouldReturnBadRequest_WhenClientIdIsEmpty()
    {
        // Arrange
        var invalidRequest = new
        {
            client_id = string.Empty,
            department_address = ValidDepartmentAddress,
            amount = ValidAmount,
            currency = ValidCurrency
        };

        // Act
        var response = await PostCashRequestAsync(invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCashRequest_ShouldReturnBadRequest_WhenDepartmentAddressIsEmpty()
    {
        // Arrange
        var invalidRequest = new
        {
            client_id = ValidClientId,
            department_address = string.Empty,  // Invalid department address
            amount = ValidAmount,
            currency = ValidCurrency
        };

        // Act
        var response = await PostCashRequestAsync(invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCashRequest_ShouldReturnBadRequest_WhenAmountIsLessThan100()
    {
        // Arrange
        var invalidRequest = new
        {
            client_id = ValidClientId,
            department_address = ValidDepartmentAddress,
            amount = 50,
            currency = ValidCurrency
        };

        // Act
        var response = await PostCashRequestAsync(invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCashRequest_ShouldReturnBadRequest_WhenAmountIsGreaterThan100000()
    {
        // Arrange
        var invalidRequest = new
        {
            client_id = ValidClientId,
            department_address = ValidDepartmentAddress,
            amount = 100001,
            currency = ValidCurrency
        };

        // Act
        var response = await PostCashRequestAsync(invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCashRequest_ShouldReturnBadRequest_WhenCurrencyIsEmpty()
    {
        // Arrange
        var invalidRequest = new
        {
            client_id = ValidClientId,
            department_address = ValidDepartmentAddress,
            amount = ValidAmount,
            currency = string.Empty
        };

        // Act
        var response = await PostCashRequestAsync(invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    #endregion


    #region Correct cash request creation tests
    
    [Fact]
    public async Task CreateCashRequest_ShouldReturnIntegerId_WhenValidDataIsProvided()
    {
        // Arrange
        var validRequest = new
        {
            client_id = ValidClientId,
            department_address = ValidDepartmentAddress,
            amount = ValidAmount,
            currency = ValidCurrency
        };

        // Act
        var response = await PostCashRequestAsync(validRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonResponse = JObject.Parse(responseBody);

        jsonResponse["id"].Type.Should().Be(JTokenType.Integer);
        int id = jsonResponse["id"].Value<int>();
        id.Should().BeGreaterThan(0);
    } 

    #endregion
    
    private async Task<HttpResponseMessage> PostCashRequestAsync(object request)
    {
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        return await _client.PostAsync("http://localhost:5000/api/cashrequest", jsonContent);
    }
}