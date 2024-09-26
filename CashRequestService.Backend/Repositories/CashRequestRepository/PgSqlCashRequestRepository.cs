using System.Data;
using CashRequestService.Backend.Entities;
using CashRequestService.Backend.Settings;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CashRequestService.Backend.Repositories.CashRequestRepository;

public class PgSqlCashRequestRepository : ICashRequestRepository
{
    private readonly string _connectionString;
    private IDbTransaction _transaction;

    public PgSqlCashRequestRepository(IOptions<RepositorySettings> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task<CashRequest> GetCreditRequest(int id)
    {
        using var connection = _transaction?.Connection ?? CreateConnection();
    
        const string query = "SELECT * FROM sp_get_cashrequest_by_id(@p_id)";

        var result = await connection.QueryAsync<CashRequest, CashRequestStatus, CashRequest>(
            query,
            (cashRequest, cashRequestStatus) =>
            {
                cashRequest.Status = cashRequestStatus;
                return cashRequest;
            },
            new { p_id = id },
            _transaction,
            splitOn: "StatusId"
        );

        return result.SingleOrDefault();
    }
    
    public async Task<IEnumerable<CashRequest>> GetCreditRequests(string clientId, string departmentAddress)
    {
        using IDbConnection connection = _transaction?.Connection ?? CreateConnection();
        const string query = "SELECT * FROM sp_get_cashrequests_by_client_department(@p_client_id, @p_department_address)";

        var result = await connection.QueryAsync<CashRequest, CashRequestStatus, CashRequest>(
            query,
            (cashRequest, cashRequestStatus) =>
            {
                cashRequest.Status = cashRequestStatus;
                return cashRequest;
            },
            new { p_client_id = clientId, p_department_address = departmentAddress },
            _transaction,
            splitOn: "StatusId"
        );

        return result;
    }

    public async Task<int> SaveCreditRequest(Entities.CashRequest request)
    {
        using IDbConnection connection = _transaction?.Connection ?? CreateConnection();
        const string storedProcedure = "sp_save_cashrequest";

        var parameters = new DynamicParameters();

        parameters.Add("p_amount", request.Amount);
        parameters.Add("p_client_id", request.ClientId);
        parameters.Add("p_department_address", request.DepartmentAddress);
        parameters.Add("p_currency", request.Currency);
        parameters.Add("p_status_id", request.Status.StatusId);

        parameters.Add("new_id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(storedProcedure, parameters, _transaction, commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("new_id");
    }
}