namespace CashRequestService.Contracts;

public record CashRequestContract(decimal Amount, string Currency, string Status);