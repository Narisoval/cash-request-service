using System.Data;
using CashRequestService.Backend.Entities;

namespace CashRequestService.Backend.Repositories.CashRequestRepository;

public interface ICashRequestRepository
{
    Task<CashRequest> GetCreditRequest(int id);
    Task<IEnumerable<CashRequest>> GetCreditRequests(string clientId, string departmentAddress);
    Task<int> SaveCreditRequest(CashRequest request);
    void SetTransaction(IDbTransaction transaction);
}