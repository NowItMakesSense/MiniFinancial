using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Accounts.Commads;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Accounts.Handlers
{
    public class RegisterAccountHandler : IRequestHandler<RegisterAccountCommand, Result<AccountDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public RegisterAccountHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, 
                                      IUserRepository userRepository, ICurrentUserService currentUser)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public async Task<Result<AccountDTO>> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            if (!_currentUser.IsAdmin && request.UserId != _currentUser.UserId) throw new OwnershipViolationException("Você não adicionar uma Conta a este usuário.");

            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null) throw new BusinessRuleException("Usuario nao cadastrado.");

            var existingAccount = await _accountRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (existingAccount is not null) throw new BusinessRuleException("Conta já cadastrada.");

            var allowNegative = user.Role.ToString() == UserRole.Admin.ToString();
            var account = new Account(request.UserId, request.Name, allowNegative, now);

            await _accountRepository.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AccountDTO(account.Id, request.UserId, user.Name, 0.0m, allowNegative);
            return Result<AccountDTO>.Success(response);
        }
    }
}
