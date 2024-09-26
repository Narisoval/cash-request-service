using System.Data;
using System.Data.SqlClient;
using CashRequestService.Backend.Entities;
using CashRequestService.Backend.Settings;
using Dapper;
using Microsoft.Extensions.Options;

namespace CashRequestService.Backend.Repositories.CashRequestRepository;

public class MsSqlCashRequestRepository : ICashRequestRepository
{
    private readonly string _connectionString;
    private IDbTransaction _transaction;

    public MsSqlCashRequestRepository(IOptions<RepositorySettings> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<CashRequest> GetCreditRequest(int id)
    {
        IDbConnection connection = _transaction?.Connection ?? CreateConnection();
        const string storedProcedure = "sp_get_cashrequest_by_id";

        IEnumerable<CashRequest> result = await connection.QueryAsync<CashRequest, CashRequestStatus, CashRequest>(
            storedProcedure,
            (cashRequest, cashRequestStatus) =>
            {
                cashRequest.Status = cashRequestStatus;
                return cashRequest;
            },
            new { Id = id },
            _transaction,
            splitOn: "StatusId",
            commandType: CommandType.StoredProcedure
        );

        return result.SingleOrDefault();
    }

    public async Task<IEnumerable<CashRequest>> GetCreditRequests(string clientId, string departmentAddress)
    {
        IDbConnection connection = _transaction?.Connection ?? CreateConnection();
        const string storedProcedure = "sp_get_cashrequests_by_client_department";

        IEnumerable<CashRequest> result = await connection.QueryAsync<CashRequest, CashRequestStatus, CashRequest>(
            storedProcedure,
            (cashRequest, cashRequestStatus) =>
            {
                cashRequest.Status = cashRequestStatus;
                return cashRequest;
            },
            new { ClientId = clientId, DepartmentAddress = departmentAddress },
            _transaction,
            splitOn: "StatusId",
            commandType: CommandType.StoredProcedure
        );

        return result;
    }

    public async Task<int> SaveCreditRequest(CashRequest request)
    {
        var connection = _transaction?.Connection ?? CreateConnection();
        string storedProcedure = "sp_save_cashrequest";
        return await connection.ExecuteScalarAsync<int>(
            storedProcedure, 
            new 
            { 
                Amount = request.Amount, 
                ClientId = request.ClientId, 
                DepartmentAddress = request.DepartmentAddress, 
                Currency = request.Currency, 
                StatusId = request.Status.StatusId 
            }, 
            _transaction, 
            commandType: CommandType.StoredProcedure
        );
    }
}