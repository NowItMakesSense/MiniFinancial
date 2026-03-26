using FluentValidation;
using MiniFinancial.Application.Features.Accounts.Commads;

namespace MiniFinancial.Application.Features.Accounts.Validators
{
    public class RemoveAccountCommandValidator : AbstractValidator<RemoveAccountCommand>
    {
        public RemoveAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O Id da Conta deve ser informado.");
        }
    }
}
