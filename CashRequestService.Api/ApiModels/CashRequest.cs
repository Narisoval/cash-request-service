using CashRequestService.Contracts;

namespace CashRequestService.Api.ApiModels;

public class CashRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set;  }
    public string Status { get; set; }

    public CashRequest(CashRequestContract contract)
    {
        this.Amount = contract.Amount;
        this.Currency = contract.Currency;
        this.Status = contract.Status;
    }
}