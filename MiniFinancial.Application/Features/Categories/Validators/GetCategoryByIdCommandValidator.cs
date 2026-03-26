using FluentValidation;
using MiniFinancial.Application.Features.Categories.Commands;

namespace MiniFinancial.Application.Features.Categories.Validators
{
    public class GetCategoryByIdCommandValidator : AbstractValidator<GetCategoryByIdCommand>
    {
        public GetCategoryByIdCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O Id da Categoria deve ser informado.");
        }
    }
}
