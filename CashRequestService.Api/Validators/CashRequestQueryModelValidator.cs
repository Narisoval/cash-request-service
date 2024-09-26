using CashRequestService.Api.ApiModels;
using FluentValidation;

namespace CashRequestService.Api.Validators;


public class CashRequestQueryModelValidator : AbstractValidator<CashRequestQueryModel>
{
    public CashRequestQueryModelValidator()
    {
        RuleFor(x => x)
            .Must(HaveRequestIdOrClientAndDepartment)
            .WithMessage("You must provide either RequestId or both ClientId and DepartmentAddress.");
    }

    private bool HaveRequestIdOrClientAndDepartment(CashRequestQueryModel model)
    {
        return model.RequestId > 0 || (!string.IsNullOrEmpty(model.ClientId) && !string.IsNullOrEmpty(model.DepartmentAddress));
    }
}