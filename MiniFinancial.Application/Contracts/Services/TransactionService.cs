using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.Features.Transactions.Commands;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Contracts.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public TransactionService(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, IDateTimeProvider dateTimeProvider)
        {
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Transaction> CreateTransactionAsync(Account accountOrigin, Account? accountDestiny, RegisterTransactionCommand request,
                                                              DateTimeOffset now, CancellationToken cancellationToken)
        {
            if (request.Type == TransactionType.Transfer)
            {
                if (!request.DestinyAccountId.HasValue || request.DestinyAccountId == Guid.Empty) throw new BusinessRuleException("Conta destino é obrigatória para transferência.");
                if (accountDestiny is null) throw new BusinessRuleException("Conta destino não encontrada.");

                var transaction = new Transaction(
                    request.OriginAccountId,
                    request.DestinyAccountId,
                    null,
                    null,
                    request.Amount,
                    request.Type,
                    request.Category,
                    request.Description,
                    now
                );

                var destinyDelta = -transaction.GetSignedAmount();
                accountDestiny.SetBalance(accountDestiny.Balance + destinyDelta, now);

                return transaction;
            }

            if (!request.CategoryUserId.HasValue) throw new BusinessRuleException("Categoria é obrigatória.");

            var category = await _categoryRepository.GetByIdAsync(request.CategoryUserId.Value, cancellationToken);
            if (category is null) throw new BusinessRuleException("Categoria não encontrada.");
            if (category.UserId != accountOrigin.UserId) throw new BusinessRuleException("Categoria não pertence ao usuário.");

            var transactions = await _transactionRepository.GetByPeriodAsync(accountOrigin.Id, _dateTimeProvider.startOfMonth(), now, cancellationToken);
            var sumAmount = transactions.Where(x => x.CategoryUserId == category.Id)
                                        .Sum(x => x.GetSignedAmount());

            if (sumAmount + request.Amount > category.MonthlyLimit) throw new BusinessRuleException("Limite da categoria excedido.");

            return new Transaction(
                request.OriginAccountId,
                null,
                request.CategoryUserId,
                category.Name,
                request.Amount,
                request.Type,
                request.Category,
                request.Description,
                now
            );
        }
    }
}
