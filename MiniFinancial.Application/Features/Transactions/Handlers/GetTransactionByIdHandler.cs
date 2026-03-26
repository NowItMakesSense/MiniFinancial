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
    public class GetTransactionByIdHandler : IRequestHandler<GetTransactionByIdCommand, Result<TransactionDTO>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICurrentUserService _currentUser;

        public GetTransactionByIdHandler(ITransactionRepository transactionRepository, IAccountRepository accountRepository, ICurrentUserService currentUser)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<TransactionDTO>> Handle(GetTransactionByIdCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (transaction is null) throw new NotFoundException("Transacao não encontrada.");

            Account? accountDestiny = null;
            var existingAccount = await _accountRepository.GetByIdAsync(transaction.OriginAccountId, cancellationToken)!;
            if (transaction.Type == TransactionType.Transfer)
            {
                accountDestiny = await _accountRepository.GetByIdAsync(transaction.DestinyAccountId!.Value, cancellationToken);
            }

            if (!_currentUser.IsAdmin)
            {
                IEnumerable<Guid> owners = accountDestiny is null ? new[] { existingAccount!.UserId } : [existingAccount!.UserId, accountDestiny.UserId];
                if (owners.Any(id => id != _currentUser.UserId)) throw new OwnershipViolationException("Você não criar uma Transacao para estes usuários.");
            }

            var response = new TransactionDTO(transaction.Id, transaction.OriginAccountId, transaction.DestinyAccountId,
                                              transaction.CategoryUserId, transaction.CategoryUserName, transaction.Amount,
                                              transaction.Type, transaction.Category, transaction.Description, transaction.OccurredAt,
                                              transaction.IsRecurring, transaction.IsReversal, transaction.ReversedTransactionId);
            return Result<TransactionDTO>.Success(response);
        }
    }
}
