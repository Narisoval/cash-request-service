namespace CashRequestService.Backend.Entities;

public class CashRequest
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string ClientId { get; set; }
    public string DepartmentAddress { get; set; }
    public string Currency { get; set; }
    public CashRequestStatus Status { get; set; }
}