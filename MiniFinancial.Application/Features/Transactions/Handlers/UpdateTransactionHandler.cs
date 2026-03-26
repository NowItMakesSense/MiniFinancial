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
    public class UpdateTransactionHandler : IRequestHandler<UpdateTransactionCommand, Result<TransactionDTO>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;
        private readonly IAccountRepository _accountRepository;

        public UpdateTransactionHandler(ITransactionRepository transactionRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, 
                                        ICurrentUserService currentUser, IAccountRepository accountRepository)
        {
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
            _accountRepository = accountRepository;
        }

        public async Task<Result<TransactionDTO>> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            var existingTransaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingTransaction is null) throw new BusinessRuleException("Transacao não encontrada.");

            Account? accountDestiny = null;
            var accountOrigin = await _accountRepository.GetByIdAsync(existingTransaction.OriginAccountId, cancellationToken);
            if (existingTransaction.Type == TransactionType.Transfer)
            {
                if (!existingTransaction.DestinyAccountId.HasValue || existingTransaction.DestinyAccountId == Guid.Empty) throw new BusinessRuleException("Conta destino é obrigatória para transferência.");

                accountDestiny = await _accountRepository.GetByIdAsync(existingTransaction.DestinyAccountId.Value, cancellationToken);
                if (accountDestiny is null) throw new BusinessRuleException("Conta destino não encontrada.");
            }

            if (!_currentUser.IsAdmin)
            {
                IEnumerable<Guid> owners = accountDestiny is null ? new[] { accountOrigin!.UserId } : [accountOrigin!.UserId, accountDestiny.UserId];
                if (owners.Any(id => id != _currentUser.UserId)) throw new OwnershipViolationException("Você não atualizar a Transacao para estes usuários.");
            }

            existingTransaction.SetDescription(request.Description, now);

            _transactionRepository.Update(existingTransaction);
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
