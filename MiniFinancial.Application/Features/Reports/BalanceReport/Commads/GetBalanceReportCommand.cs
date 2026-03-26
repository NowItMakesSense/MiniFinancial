using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Reports.BalanceReport.Commads
{
    public record GetBalanceReportCommand(Guid UserId, int Year) : IRequest<Result<GetBalanceReportDTO>>;
}
