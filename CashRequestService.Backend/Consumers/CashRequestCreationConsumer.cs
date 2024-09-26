using CashRequestService.Backend.Entities;
using CashRequestService.Backend.Services.UnitOfWork;
using CashRequestService.Backend.Settings;
using CashRequestService.Contracts;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CashRequestService.Backend.Consumers;

public class CashRequestCreationConsumer : IConsumer<CashRequestCreationContract>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CashRequestSettings _settings;
    private readonly ILogger<CashRequestCreationConsumer> _logger;

    public CashRequestCreationConsumer(ILogger<CashRequestCreationConsumer> logger, IUnitOfWork unitOfWork, IOptions<CashRequestSettings> settings)
    {
        _logger = logger;
        this._unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        this._settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }
    
    public async Task Consume(ConsumeContext<CashRequestCreationContract> context)
    {
        CashRequestCreationContract contract = context.Message;

        _logger.LogWarning("CashRequest contact has been received {data}", JsonConvert.SerializeObject(contract));

        var cashRequest = new CashRequest()
        {
            Amount = contract.Amount,
            ClientId = contract.ClientId,
            DepartmentAddress = contract.DepartmentAddress,
            Currency = contract.Currency,
            Status = new CashRequestStatus()
            {
                StatusId = this._settings.InitialStatusId
            }
            
        };
        
        int id = await _unitOfWork.CashRequests.SaveCreditRequest(cashRequest);
        
        await context.RespondAsync(new CashRequestCreationResponseContract(id));
    }
}