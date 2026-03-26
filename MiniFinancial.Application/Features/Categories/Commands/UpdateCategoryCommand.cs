using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Categories.Commands
{
    public record UpdateCategoryCommand(Guid Id, string Name, decimal? MonthlyLimit) : IRequest<Result<CategoryDTO>>;
}
