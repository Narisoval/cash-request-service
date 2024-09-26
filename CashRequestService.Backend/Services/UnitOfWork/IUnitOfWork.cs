using CashRequestService.Backend.Repositories.CashRequestRepository;

namespace CashRequestService.Backend.Services.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    ICashRequestRepository CashRequests { get; }
    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();
}