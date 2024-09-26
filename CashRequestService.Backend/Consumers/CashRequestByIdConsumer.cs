using CashRequestService.Backend.Entities;
using CashRequestService.Backend.Services.UnitOfWork;
using CashRequestService.Contracts;
using MassTransit;

namespace CashRequestService.Backend.Consumers;

public class CashRequestByIdConsumer : IConsumer<CashRequestByIdContract>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CashRequestByIdConsumer> _logger;

    public CashRequestByIdConsumer(ILogger<CashRequestByIdConsumer> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Consume(ConsumeContext<CashRequestByIdContract> context)
    {
        CashRequestByIdContract contract = context.Message;
        
        _logger.LogWarning("Received CashRequestByIdContract: {requestId}", contract.RequestId);

        CashRequest cashRequest = await _unitOfWork.CashRequests.GetCreditRequest(contract.RequestId);

        if (cashRequest == null)
        {
            await context.RespondAsync(new CashRequestListContract(new List<CashRequestContract>()));
            return;
        }

        var result = new List<CashRequestContract>
        {
            new CashRequestContract(
                cashRequest.Amount, 
                cashRequest.Currency, 
                cashRequest.Status.StatusName
            )
        };

        await context.RespondAsync(new CashRequestListContract(result));
    }
}
