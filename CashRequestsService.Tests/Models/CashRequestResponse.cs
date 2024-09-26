using Newtonsoft.Json;

namespace CashRequestsService.Tests.Models;

public class CashRequestResponse
{
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }
}