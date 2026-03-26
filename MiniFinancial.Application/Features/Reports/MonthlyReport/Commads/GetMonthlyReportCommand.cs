using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Reports.MonthlyReport.Commads
{
    public record GetMonthlyReportCommand(Guid UserId, int Month, int Year) : IRequest<Result<GetMonthlyReportDTO>>;
}
