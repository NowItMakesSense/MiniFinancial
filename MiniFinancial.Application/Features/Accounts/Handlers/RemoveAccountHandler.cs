using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Accounts.Commads;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Accounts.Handlers
{
    public class RemoveAccountHandler : IRequestHandler<RemoveAccountCommand, Result<AccountDTO>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public RemoveAccountHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork, 
                                    IDateTimeProvider dateTimeProvider, ICurrentUserService currentUser)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public async Task<Result<AccountDTO>> Handle(RemoveAccountCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            var existingAccount = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingAccount is null) throw new BusinessRuleException("Conta nao cadastrada.");
            if (!_currentUser.IsAdmin && existingAccount.UserId != _currentUser.UserId) throw new OwnershipViolationException("Você não pode remover a conta deste usuário.");

            existingAccount.Delete(now);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AccountDTO(null, existingAccount.UserId, existingAccount.Name, existingAccount.Balance, existingAccount.AllowNegativeBalance);
            return Result<AccountDTO>.Success(response);
        }
    }
}