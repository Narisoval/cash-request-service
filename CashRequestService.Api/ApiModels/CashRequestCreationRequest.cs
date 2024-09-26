using System.Text.Json.Serialization;

namespace CashRequestService.Api.ApiModels;

public class CashRequestCreationRequest
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; }

    [JsonPropertyName("department_address")]
    public string DepartmentAddress { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }
}