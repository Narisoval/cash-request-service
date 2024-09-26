using System.Text;
using Newtonsoft.Json;

namespace CashRequestsService.Tests;

public static class CashRequestInitializer
{
    private static bool DataInitialized = false;

    public static async Task InitializeAsync(HttpClient client)
    {
        if (!DataInitialized)
        {
            var cashRequests = new[]
            {
                new { client_id = "client1", department_address = "123 Main St", amount = 1000, currency = "USD" },
                new { client_id = "client1", department_address = "123 Main St", amount = 1000, currency = "USD" },
                new { client_id = "client2", department_address = "456 Elm St", amount = 1000, currency = "USD" }
            };

            foreach (var request in cashRequests)
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                await client.PostAsync("http://localhost:5000/api/cashrequest", jsonContent);
            }

            DataInitialized = true;
        }
    }
}