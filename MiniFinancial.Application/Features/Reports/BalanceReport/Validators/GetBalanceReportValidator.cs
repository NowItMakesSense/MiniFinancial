using FluentValidation;
using MiniFinancial.Application.Features.Reports.BalanceReport.Commads;

namespace MiniFinancial.Application.Features.Reports.BalanceReport.Validators
{
    public class GetBalanceReportValidator : AbstractValidator<GetBalanceReportCommand>
    {
        public GetBalanceReportValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("O Id da Categoria deve ser informado.");

            RuleFor(x => x.Year)
                .NotEmpty().WithMessage("O ano deve ser informado.");
        }
    }
}
