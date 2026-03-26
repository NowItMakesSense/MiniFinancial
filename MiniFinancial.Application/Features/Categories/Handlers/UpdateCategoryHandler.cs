using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Categories.Commands;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Categories.Handlers
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDTO>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public UpdateCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, ICurrentUserService currentUser)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public async Task<Result<CategoryDTO>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingCategory is null) throw new BusinessRuleException("Categoria não encontrada.");
            if (!_currentUser.IsAdmin && existingCategory.UserId != _currentUser.UserId) throw new OwnershipViolationException("Você não alterar a Categoria deste usuário.");

            existingCategory.Rename(request.Name, now);
            existingCategory.SetMonthlyLimit(request.MonthlyLimit, now);

            _categoryRepository.Update(existingCategory);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CategoryDTO(request.Id, existingCategory.UserId, existingCategory.Name, existingCategory.MonthlyLimit);
            return Result<CategoryDTO>.Success(response);
        }
    }
}
