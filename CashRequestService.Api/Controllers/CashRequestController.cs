using CashRequestService.Api.ApiModels;
using CashRequestService.Api.Settings;
using CashRequestService.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CashRequestService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CashRequestController : ControllerBase
{
    private readonly ILogger<CashRequestController> _logger;
    private readonly IRequestClient<CashRequestCreationContract> _cashRequestCreationClient;
    private readonly IRequestClient<CashRequestByIdContract> _cashRequestByIdRequestClient;
    private readonly IRequestClient<CashRequestByClientAndDepartmentContract> _cashRequestByClientIdAndAddressRequestClient;
    
    private readonly RequestTimeout _requestTimeout;

    public CashRequestController(IRequestClient<CashRequestCreationContract> cashRequestCreationClient, 
        ILogger<CashRequestController> logger, 
        IOptions<MessageBrokerSettings> messageBrokerSettings, IRequestClient<CashRequestByIdContract> cashRequestByIdRequestClient, IRequestClient<CashRequestByClientAndDepartmentContract> cashRequestByClientIdAndAddressRequestClient)
    {
        _cashRequestCreationClient = cashRequestCreationClient;
        _logger = logger;
        _cashRequestByIdRequestClient = cashRequestByIdRequestClient;
        _cashRequestByClientIdAndAddressRequestClient = cashRequestByClientIdAndAddressRequestClient;
        _requestTimeout = RequestTimeout.After(s: messageBrokerSettings.Value.TimeOutInSeconds);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCashRequest([FromBody] CashRequestCreationRequest cashRequestCreationRequest, CancellationToken token)
    {
        var contract = new CashRequestCreationContract(
            cashRequestCreationRequest.ClientId, 
            cashRequestCreationRequest.DepartmentAddress, 
            cashRequestCreationRequest.Amount, 
            cashRequestCreationRequest.Currency
        );

        Response<CashRequestCreationResponseContract> response = 
            await _cashRequestCreationClient.GetResponse<CashRequestCreationResponseContract>(contract, token, _requestTimeout);

        return Ok(new { id = response.Message.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetCashRequests([FromQuery] CashRequestQueryModel queryModel, CancellationToken token)
    {
        IEnumerable<CashRequest> cashRequests = null;
        
        if (queryModel.RequestId > 0)
        {
            CashRequestByIdContract contract = new CashRequestByIdContract(queryModel.RequestId);
            
            Response<CashRequestListContract> response = await _cashRequestByIdRequestClient.GetResponse<CashRequestListContract>(contract, token, _requestTimeout);
            
            cashRequests = response.Message?.List?.Select(x => new CashRequest(x));
        }
        if(!string.IsNullOrEmpty(queryModel.DepartmentAddress) && !string.IsNullOrEmpty(queryModel.ClientId))
        {
            CashRequestByClientAndDepartmentContract contract = new CashRequestByClientAndDepartmentContract(queryModel.ClientId, queryModel.DepartmentAddress);
            
            Response<CashRequestListContract> response = await _cashRequestByClientIdAndAddressRequestClient.GetResponse<CashRequestListContract>(contract, token, _requestTimeout);

            cashRequests = response.Message?.List?.Select(x => new CashRequest(x));
        }

        if (cashRequests?.Any() != true)
        {
            return NotFound();
        }

        return Ok(cashRequests);
    }
}