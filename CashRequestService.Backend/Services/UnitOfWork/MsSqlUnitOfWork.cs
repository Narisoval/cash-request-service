using System.Data;
using System.Data.SqlClient;
using CashRequestService.Backend.Repositories.CashRequestRepository;
using CashRequestService.Backend.Settings;
using Microsoft.Extensions.Options;

namespace CashRequestService.Backend.Services.UnitOfWork;

public class MsSqlUnitOfWork : UnitOfWorkBase
{
    private readonly MsSqlCashRequestRepository _cashRequestRepository;

    public MsSqlUnitOfWork(IOptions<RepositorySettings> options) : base(options.Value.ConnectionString)
    {
        _cashRequestRepository = new MsSqlCashRequestRepository(options);
    }

    public override ICashRequestRepository CashRequests => _cashRequestRepository;

    protected override IDbConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }
}