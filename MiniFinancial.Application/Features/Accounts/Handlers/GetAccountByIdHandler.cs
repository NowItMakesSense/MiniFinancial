using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Accounts.Commads;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Accounts.Handlers
{
    public class GetAccountByIdHandler : IRequestHandler<GetAccountByIdCommand, Result<AccountDTO>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICurrentUserService _currentUser;

        public GetAccountByIdHandler(IAccountRepository accountRepository, ICurrentUserService currentUser)
        {
            _accountRepository = accountRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<AccountDTO>> Handle(GetAccountByIdCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);
            if (account is null) throw new NotFoundException("Conta não encontrada.");
            if (!_currentUser.IsAdmin && account.UserId != _currentUser.UserId) throw new OwnershipViolationException("Você não ver esta Conta.");

            var response = new AccountDTO(account.Id, account.UserId, account.Name, account.Balance, account.AllowNegativeBalance);
            return Result<AccountDTO>.Success(response);
        }
    }
}
