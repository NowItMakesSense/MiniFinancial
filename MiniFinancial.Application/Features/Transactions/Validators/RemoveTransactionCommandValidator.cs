using FluentValidation;
using MiniFinancial.Application.Features.Transactions.Commands;

namespace MiniFinancial.Application.Features.Transactions.Validators
{
    public class RemoveTransactionCommandValidator : AbstractValidator<RemoveTransactionCommand>
    {
        public RemoveTransactionCommandValidator()
        {
            RuleFor(x => x.Id)
                 .NotEmpty().WithMessage("O Id da Transacao deve ser informado.");
        }
    }
}
