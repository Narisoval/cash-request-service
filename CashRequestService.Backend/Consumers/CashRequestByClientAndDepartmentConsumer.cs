using CashRequestService.Backend.Entities;
using CashRequestService.Backend.Services.UnitOfWork;
using CashRequestService.Contracts;
using MassTransit;

namespace CashRequestService.Backend.Consumers;

public class CashRequestByClientAndDepartmentConsumer : IConsumer<CashRequestByClientAndDepartmentContract>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CashRequestByClientAndDepartmentConsumer> _logger;

    public CashRequestByClientAndDepartmentConsumer(ILogger<CashRequestByClientAndDepartmentConsumer> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Consume(ConsumeContext<CashRequestByClientAndDepartmentContract> context)
    {
        CashRequestByClientAndDepartmentContract contract = context.Message;
        _logger.LogWarning("Received CashRequestByClientAndDepartmentContract for ClientId: {clientId}, DepartmentAddress: {departmentAddress}", 
            contract.ClientId, contract.DepartmentAddress);

        IEnumerable<CashRequest> cashRequests = await _unitOfWork.CashRequests.GetCreditRequests(contract.ClientId, contract.DepartmentAddress);

        IList<CashRequestContract> result = cashRequests
            .Select(x => new CashRequestContract(
                x.Amount, 
                x.Currency, 
                x.Status.StatusName
            ))
            .ToList();

        await context.RespondAsync(new CashRequestListContract(result));
    }
}
