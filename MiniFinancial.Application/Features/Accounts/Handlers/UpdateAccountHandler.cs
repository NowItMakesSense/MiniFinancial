using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Accounts.Commads;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Accounts.Handlers
{
    public class UpdateAccountHandler : IRequestHandler<UpdateAccountCommand, Result<AccountDTO>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public UpdateAccountHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork, 
                                    IDateTimeProvider dateTimeProvider, ICurrentUserService currentUser)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public async Task<Result<AccountDTO>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            var existingAccount = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingAccount is null) throw new BusinessRuleException("Conta não encontrada.");
            if (!_currentUser.IsAdmin && existingAccount.UserId != _currentUser.UserId) throw new OwnershipViolationException("Você não pode alterar a conta deste usuário.");

            existingAccount.Rename(request.Name, now);

            _accountRepository.Update(existingAccount);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AccountDTO(request.Id, existingAccount.UserId, existingAccount.Name, existingAccount.Balance, existingAccount.AllowNegativeBalance);
            return Result<AccountDTO>.Success(response);
        }
    }
}
