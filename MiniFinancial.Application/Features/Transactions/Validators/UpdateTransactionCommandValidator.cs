using FluentValidation;
using MiniFinancial.Application.Features.Transactions.Commands;

namespace MiniFinancial.Application.Features.Transactions.Validators
{
    public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
    {
        public UpdateTransactionCommandValidator() 
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O Id da Transacao deve ser informado.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("A descricao é obrigatória.")
                .MaximumLength(300).WithMessage("A descricao não pode exceder 150 caracteres.");
        }
    }
}
