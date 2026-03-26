using FluentValidation;
using MiniFinancial.Application.Features.Categories.Commands;

namespace MiniFinancial.Application.Features.Categories.Validators
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O Id da Categoria deve ser informado.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(150).WithMessage("O nome não pode exceder 150 caracteres.");

            RuleFor(x => x.MonthlyLimit)
                .GreaterThanOrEqualTo(0.0m).WithMessage("O limite nao pode ser negativo.");
        }
    }
}
