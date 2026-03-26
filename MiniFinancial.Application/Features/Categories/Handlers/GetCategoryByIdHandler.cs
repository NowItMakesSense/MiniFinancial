using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Categories.Commands;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Categories.Handlers
{
    public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdCommand, Result<CategoryDTO>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICurrentUserService _currentUser;

        public GetCategoryByIdHandler(ICategoryRepository categoryRepository, ICurrentUserService currentUser)
        {
            _categoryRepository = categoryRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<CategoryDTO>> Handle(GetCategoryByIdCommand request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null) throw new NotFoundException("Categoria não encontrada.");
            if (!_currentUser.IsAdmin && category.UserId != _currentUser.UserId) throw new OwnershipViolationException("Você não ver esta Categoria do usuário.");

            var response = new CategoryDTO(category.Id, category.UserId, category.Name, category.MonthlyLimit);
            return Result<CategoryDTO>.Success(response);
        }
    }
}
