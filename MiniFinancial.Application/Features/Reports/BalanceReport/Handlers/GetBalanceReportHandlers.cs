using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Reports.BalanceReport.Commads;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Reports.BalanceReport.Handlers
{
    public class GetBalanceReportHandlers : IRequestHandler<GetBalanceReportCommand, Result<GetBalanceReportDTO>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetBalanceReportHandlers(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<GetBalanceReportDTO>> Handle(GetBalanceReportCommand request, CancellationToken cancellationToken)
        {
            var primeiroDiaAno = new DateTime(request.Year, 1, 1, 00, 00, 00, DateTimeKind.Utc);
            var primeiroDiaProximoAno = primeiroDiaAno.AddYears(1);

            if (!_currentUser.IsAdmin && request.UserId != _currentUser.UserId) throw new OwnershipViolationException("Você não pode ver os relatorios deste usuário.");

            var getUserAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == request.UserId && !a.IsDeleted, cancellationToken);
            if (getUserAccount is null) throw new BusinessRuleException("Conta nao encontrada.");

            var transactions = await _context.Transactions.Where(t => (t.OriginAccountId == getUserAccount.Id || t.DestinyAccountId == getUserAccount.Id) 
                                                                       && t.OccurredAt >= primeiroDiaAno 
                                                                       && t.OccurredAt < primeiroDiaProximoAno).ToListAsync(cancellationToken);

            var incomes = transactions.Where(t => t.Type == TransactionType.Income).ToList();
            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
            var transfers = transactions.Where(t => t.Type == TransactionType.Transfer).ToList();

            var totalIncome = incomes.Sum(t => t.Amount);
            var totalExpense = expenses.Sum(t => t.Amount);

            var transfersToOtherAccounts = transfers.Where(t => t.DestinyAccountId != getUserAccount.Id).ToList();
            var transfersWithinUserAccounts = transfers.Where(t => t.DestinyAccountId == getUserAccount.Id).ToList();

            var totalTransfersToOtherAccounts = transfersToOtherAccounts.Sum(t => t.Amount);
            var totalTransfersWithinUserAccounts = transfersWithinUserAccounts.Sum(t => t.Amount);

            var countTransfersToOtherAccounts = transfersToOtherAccounts.Count();
            var countTransfersWithinUserAccounts = transfersWithinUserAccounts.Count();

            var totalBalance = incomes.Sum(t => t.Amount) + transfersWithinUserAccounts.Sum(t => t.Amount) -
                               expenses.Sum(t => t.Amount) - transfersToOtherAccounts.Sum(t => t.Amount);

            var response = new GetBalanceReportDTO(totalIncome, 
                                                   totalExpense, 
                                                   totalTransfersWithinUserAccounts, 
                                                   totalTransfersToOtherAccounts,
                                                   totalBalance,
                                                   totalTransfersWithinUserAccounts - totalTransfersToOtherAccounts,
                                                   incomes.Count, 
                                                   expenses.Count,
                                                   countTransfersWithinUserAccounts,
                                                   countTransfersToOtherAccounts);
            return Result<GetBalanceReportDTO>.Success(response);
        }
    }
}