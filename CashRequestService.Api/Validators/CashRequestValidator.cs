using CashRequestService.Api.ApiModels;
using FluentValidation;

namespace CashRequestService.Api.Validators;

public class CashRequestValidator : AbstractValidator<CashRequestCreationRequest>
{
    public CashRequestValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required.");

        RuleFor(x => x.DepartmentAddress)
            .NotEmpty().WithMessage("Department address is required.");

        RuleFor(x => x.Amount)
            .InclusiveBetween(100, 100000)
            .WithMessage("Amount must be between 100 and 100,000.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.");
    }
}