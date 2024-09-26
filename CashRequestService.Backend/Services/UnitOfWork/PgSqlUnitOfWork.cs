using System.Data;
using CashRequestService.Backend.Repositories.CashRequestRepository;
using CashRequestService.Backend.Settings;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CashRequestService.Backend.Services.UnitOfWork;

public class PgSqlUnitOfWork : UnitOfWorkBase
{
    private readonly PgSqlCashRequestRepository _cashRequestRepository;

    public PgSqlUnitOfWork(IOptions<RepositorySettings> options) : base(options.Value.ConnectionString)
    {
        _cashRequestRepository = new PgSqlCashRequestRepository(options);
    }

    public override ICashRequestRepository CashRequests => _cashRequestRepository;

    protected override IDbConnection CreateConnection(string connectionString)
    {
        return new NpgsqlConnection(connectionString);
    }
}