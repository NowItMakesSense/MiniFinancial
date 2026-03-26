using FluentValidation;
using MiniFinancial.Application.Features.Transactions.Commands;
using MiniFinancial.Domain.Enums;

namespace MiniFinancial.Application.Features.Transactions.Validators
{
    public class RegisterTransactionCommandValidator : AbstractValidator<RegisterTransactionCommand>
    {
        public RegisterTransactionCommandValidator()
        {
            RuleFor(x => x.OriginAccountId)
                .NotEmpty().WithMessage("O Id da Conta Origem deve ser informado.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("A descricao é obrigatória.")
                .MaximumLength(300).WithMessage("A descricao não pode exceder 150 caracteres.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("O tipo da transacao deve ser informada.")
                .IsInEnum().WithMessage("Tipo inválido.");

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0.0m).WithMessage("O valor nao pode ser negativo.");

            When(x => x.Type == TransactionType.Transfer, () =>
            {
                RuleFor(x => x.DestinyAccountId)
                    .NotNull().WithMessage("Conta destino é obrigatória para transferência.")
                    .NotEqual(Guid.Empty).WithMessage("Conta destino inválida.");
            });

            When(x => x.Type != TransactionType.Transfer, () =>
            {
                RuleFor(x => x.CategoryUserId)
                    .NotNull().WithMessage("Categoria é obrigatória para esse tipo de transação.");
            });
        }
    }
}
