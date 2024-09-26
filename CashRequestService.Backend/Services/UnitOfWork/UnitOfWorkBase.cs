using System.Data;
using CashRequestService.Backend.Repositories.CashRequestRepository;

namespace CashRequestService.Backend.Services.UnitOfWork;

public abstract class UnitOfWorkBase : IUnitOfWork
{
    protected readonly IDbConnection _connection;
    protected IDbTransaction _transaction;

    public UnitOfWorkBase(string connectionString)
    {
        _connection = CreateConnection(connectionString);
    }

    protected abstract IDbConnection CreateConnection(string connectionString);

    public abstract ICashRequestRepository CashRequests { get; }

    public void BeginTransaction()
    {
        if (_transaction == null)
        {
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }
    }

    public void CommitTransaction()
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            Dispose();
        }
    }

    public void RollbackTransaction()
    {
        _transaction?.Rollback();
    }

    public void Dispose()
    {
        if (_transaction != null)
        {
            _transaction.Dispose();
            _transaction = null;
        }

        if (_connection.State == ConnectionState.Open)
        {
            _connection.Close();
        }
    }
}
