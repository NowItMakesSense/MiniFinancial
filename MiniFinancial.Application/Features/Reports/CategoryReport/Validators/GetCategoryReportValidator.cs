using FluentValidation;
using MiniFinancial.Application.Features.Reports.CategoryReport.Commads;

namespace MiniFinancial.Application.Features.Reports.CategoryReport.Validators
{
    public class GetCategoryReportValidator : AbstractValidator<GetCategoryReportCommand>
    {
        public GetCategoryReportValidator()
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
