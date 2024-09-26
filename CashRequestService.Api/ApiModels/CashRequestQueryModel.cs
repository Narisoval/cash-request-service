using Microsoft.AspNetCore.Mvc;

namespace CashRequestService.Api.ApiModels;

public class CashRequestQueryModel
{
    [FromQuery(Name = "request_id")]
    public int RequestId { get; set; }
    
    [FromQuery(Name = "client_id")]
    public string ClientId { get; set; }
    
    [FromQuery(Name = "department_address")]
    public string DepartmentAddress { get; set; }
}