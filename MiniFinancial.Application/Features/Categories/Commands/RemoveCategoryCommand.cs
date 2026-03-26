using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.DTOs;

namespace MiniFinancial.Application.Features.Categories.Commands
{
    public record RemoveCategoryCommand(Guid Id) : IRequest<Result<CategoryDTO>>;
}
