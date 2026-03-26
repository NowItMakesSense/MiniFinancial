using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Reports.CategoryReport.Commads;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Reports.CategoryReport.Handlers
{
    public class GetCategoryReportHandler : IRequestHandler<GetCategoryReportCommand, Result<GetCategoryReportDTO>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetCategoryReportHandler(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Result<GetCategoryReportDTO>> Handle(GetCategoryReportCommand request, CancellationToken cancellationToken)
        {
            var getUltimoDiaMes = DateTime.DaysInMonth(request.Year, request.Month);
            var primeiroDiaMes = new DateTime(request.Year, request.Month, 1, 00, 00, 00, DateTimeKind.Utc);
            var ultimoDiaMes = new DateTime(request.Year, request.Month, getUltimoDiaMes, 23, 59, 59, DateTimeKind.Utc);

            if (!_currentUser.IsAdmin && request.UserId != _currentUser.UserId) throw new OwnershipViolationException("Você não pode ver os relatorios deste usuário.");

            var getUserAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == request.UserId && !a.IsDeleted, cancellationToken);
            if (getUserAccount is null) throw new BusinessRuleException("Conta nao encontrada.");

            var userCategories = await _context.Categories.Where(c => c.UserId == request.UserId)
                                                          .Select(c => c.Id)
                                                          .ToListAsync(cancellationToken);
            if(!userCategories.Any()) throw new BusinessRuleException("Nenhuma categoria cadastrada para essa Conta.");

            var transactions = await _context.Transactions.Where(t => (t.OriginAccountId == getUserAccount.Id || t.DestinyAccountId == getUserAccount.Id)
                                                                       && userCategories.Contains((Guid)t.CategoryUserId!)
                                                                       && t.Type != TransactionType.Transfer
                                                                       && t.OccurredAt >= primeiroDiaMes
                                                                       && t.OccurredAt < ultimoDiaMes).ToListAsync(cancellationToken);

            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            var response = new GetCategoryReportDTO(income, expense, income - expense, transactions.Count);
            return Result<GetCategoryReportDTO>.Success(response);
        }
    }
}
