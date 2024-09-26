namespace CashRequestService.Api.Settings;

public class MessageBrokerSettings
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int TimeOutInSeconds { get; set; }
}