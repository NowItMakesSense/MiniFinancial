using FluentValidation;
using MiniFinancial.Application.Features.Reports.MonthlyReport.Commads;

namespace MiniFinancial.Application.Features.Reports.MonthlyReport.Validators
{
    public class GetMonthlyReportValidator : AbstractValidator<GetMonthlyReportCommand>
    {
        public GetMonthlyReportValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("O Id da Categoria deve ser informado.");

            RuleFor(x => x.Month)
                 .NotEmpty().WithMessage("O mes deve ser informado.");

            RuleFor(x => x.Year)
                .NotEmpty().WithMessage("O ano deve ser informado.");
        }
    }
}
