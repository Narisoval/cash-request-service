namespace CashRequestService.Contracts;

public record CashRequestCreationContract(string ClientId, string DepartmentAddress, decimal Amount, string Currency);
