using FluentValidation;
using MiniFinancial.Application.Features.Accounts.Commads;

namespace MiniFinancial.Application.Features.Accounts.Validators
{
    public class RegisterAccountCommandValidator : AbstractValidator<RegisterAccountCommand>
    {
        public RegisterAccountCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("O Id do Usuario deve ser informado para criar a Conta.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(150).WithMessage("O nome não pode exceder 150 caracteres.");
        }
    }
}
