using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.Contracts.Services;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Transactions.Commands;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Transactions.Handlers
{
    public class RegisterTransactionHandler : IRequestHandler<RegisterTransactionCommand, Result<TransactionDTO>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;
        private readonly ITransactionService _transactionService;

        public RegisterTransactionHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork,
                                          IDateTimeProvider dateTimeProvider, ICurrentUserService currentUser, ITransactionService transactionService)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
            _transactionService = transactionService;
        }

        public async Task<Result<TransactionDTO>> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;

            var accountOrigin = await _accountRepository.GetByIdAsync(request.OriginAccountId, cancellationToken);
            if (accountOrigin is null) throw new BusinessRuleException("Conta origem não encontrada.");
            if (request.Type != TransactionType.Income && accountOrigin.Balance < request.Amount) throw new BusinessRuleException("Saldo insuficiente.");

            Account? accountDestiny = null;
            if (request.Type == TransactionType.Transfer)
            {
                if (!request.DestinyAccountId.HasValue || request.DestinyAccountId == Guid.Empty) throw new BusinessRuleException("Conta destino é obrigatória para transferência.");

                accountDestiny = await _accountRepository.GetByIdAsync(request.DestinyAccountId.Value, cancellationToken);
                if (accountDestiny is null) throw new BusinessRuleException("Conta destino não encontrada.");
            }

            if (!_currentUser.IsAdmin)
            {
                IEnumerable<Guid> owners = accountDestiny is null ? new[] { accountOrigin.UserId } : [accountOrigin.UserId, accountDestiny.UserId];
                if (owners.Any(id => id != _currentUser.UserId)) throw new OwnershipViolationException("Você não criar uma Transacao para estes usuários.");
            }

            var newTransaction = await _transactionService.CreateTransactionAsync(accountOrigin, accountDestiny, request, now, cancellationToken);
            accountOrigin.SetBalance(accountOrigin.Balance + newTransaction.GetSignedAmount(), now);
            _accountRepository.Update(accountOrigin);

            if (accountDestiny is not null) _accountRepository.Update(accountDestiny); 
            
            await _transactionRepository.AddAsync(newTransaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<TransactionDTO>.Success(
                new TransactionDTO(
                    newTransaction.Id,
                    newTransaction.OriginAccountId,
                    newTransaction.DestinyAccountId,
                    newTransaction.CategoryUserId,
                    newTransaction.CategoryUserName,
                    newTransaction.Amount,
                    newTransaction.Type,
                    newTransaction.Category,
                    newTransaction.Description,
                    newTransaction.OccurredAt,
                    newTransaction.IsRecurring,
                    newTransaction.IsReversal,
                    newTransaction.ReversedTransactionId
                )
            );
        }
    }
}
