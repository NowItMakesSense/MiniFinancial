using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Reports.CategoryReport.Commads
{
    public record GetCategoryReportCommand(Guid UserId, int Month, int Year) : IRequest<Result<GetCategoryReportDTO>>;
}
