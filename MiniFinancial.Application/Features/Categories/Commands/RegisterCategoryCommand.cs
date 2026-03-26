using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Categories.Commands
{
    public record RegisterCategoryCommand(Guid UserId, string Name, decimal? MonthlyLimit) : IRequest<Result<CategoryDTO>>;
}
