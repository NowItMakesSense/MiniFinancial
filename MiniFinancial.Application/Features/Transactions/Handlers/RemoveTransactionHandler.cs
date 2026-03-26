using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Transactions.Commands;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Transactions.Handlers
{
    public class RemoveTransactionHandler : IRequestHandler<RemoveTransactionCommand, Result<TransactionDTO>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public RemoveTransactionHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepository,
                                        IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, ICurrentUserService currentUser)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public async Task<Result<TransactionDTO>> Handle(RemoveTransactionCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            var existingTransaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingTransaction is null) throw new BusinessRuleException("Transacao nao encontrada.");
            if(existingTransaction.Type == TransactionType.Income) throw new BusinessRuleException("Esse tipo de Transacao nao pode ser revertida.");

            Account? accountDestiny = null;
            var originAccount = await _accountRepository.GetByIdAsync(existingTransaction.OriginAccountId, cancellationToken);
            if (originAccount is null) throw new BusinessRuleException("Conta Origem nao encontrada.");

            if (existingTransaction.Type == TransactionType.Transfer)
            {
                accountDestiny = await _accountRepository.GetByIdAsync(existingTransaction.DestinyAccountId!.Value, cancellationToken);
                if (accountDestiny is null) throw new BusinessRuleException("Conta Destino não encontrada.");
            }

            if (!_currentUser.IsAdmin)
            {
                IEnumerable<Guid> owners = accountDestiny is null ? new[] { originAccount!.UserId } : [originAccount!.UserId, accountDestiny.UserId];
                if (owners.Any(id => id != _currentUser.UserId)) throw new OwnershipViolationException("Você não remover/reverter a Transacao para estes usuários.");
            }

            //Cria uma nova transacao para reverter a antiga.
            var newTransation = existingTransaction.Reverse(now);
            newTransation.SetReversal(true, now);
            newTransation.SetReversedTransactionId(existingTransaction.Id, now);
            newTransation.SetDescription(@$"Reversal : ${newTransation.Description}", now);

            if (accountDestiny is not null)
            {
                var newDestinyBalance = accountDestiny!.Balance - newTransation.Amount;

                accountDestiny.SetBalance(newDestinyBalance, now);
                _accountRepository.Update(accountDestiny);
            }

            var newBalance = originAccount.Balance + existingTransaction.Amount;
            originAccount.SetBalance(newBalance, now);

            _accountRepository.Update(originAccount);
            _transactionRepository.Update(existingTransaction);

            await _transactionRepository.AddAsync(newTransation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new TransactionDTO(existingTransaction.Id, existingTransaction.OriginAccountId, existingTransaction.DestinyAccountId,
                                              existingTransaction.CategoryUserId,
                                              existingTransaction.CategoryUserName, existingTransaction.Amount,
                                              existingTransaction.Type, existingTransaction.Category, existingTransaction.Description, existingTransaction.OccurredAt,
                                              existingTransaction.IsRecurring, existingTransaction.IsReversal, existingTransaction.ReversedTransactionId);
            return Result<TransactionDTO>.Success(response);
        }
    }
}
