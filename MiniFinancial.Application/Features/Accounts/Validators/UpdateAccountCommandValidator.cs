using FluentValidation;
using MiniFinancial.Application.Features.Accounts.Commads;

namespace MiniFinancial.Application.Features.Accounts.Validators
{
    public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
    {
        public UpdateAccountCommandValidator() 
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O Id da Conta deve ser informado.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(150).WithMessage("O nome não pode exceder 150 caracteres.");
        }
    }
}
