using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Categories.Commands;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Categories.Handlers
{
    public class RemoveCategoryHandler : IRequestHandler<RemoveCategoryCommand, Result<CategoryDTO>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public RemoveCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, ICurrentUserService currentUser)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public async Task<Result<CategoryDTO>> Handle(RemoveCategoryCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingCategory is null) throw new BusinessRuleException("Categoria nao cadastrada.");
            if (!_currentUser.IsAdmin && existingCategory.UserId != _currentUser.UserId) throw new OwnershipViolationException("Você não excluir a Categoria deste usuário.");

            existingCategory.Delete(now);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CategoryDTO(existingCategory.Id, existingCategory.UserId, existingCategory.Name,
                                           existingCategory.MonthlyLimit);
            return Result<CategoryDTO>.Success(response);
        }
    }
}
