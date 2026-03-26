using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Categories.Commands;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Categories.Handlers
{
    public class RegisterCategoryHandler : IRequestHandler<RegisterCategoryCommand, Result<CategoryDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public RegisterCategoryHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork,
                                       IDateTimeProvider dateTimeProvider, IUserRepository userRepository, ICurrentUserService currentUser)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _userRepository = userRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<CategoryDTO>> Handle(RegisterCategoryCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null) throw new BusinessRuleException("Usuario nao cadastrado.");
            if (!_currentUser.IsAdmin && user.Id != _currentUser.UserId) throw new OwnershipViolationException("Você não cadastrar uma Categoria para este usuário.");

            var userCategories = await _categoryRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (userCategories.Where(x => x.Name == request.Name)!.Any()) throw new BusinessRuleException("Categoria já cadastrada.");

            var account = new Category(request.UserId, request.Name, request.MonthlyLimit, now);

            await _categoryRepository.AddAsync(account, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CategoryDTO(account.Id, account.UserId, account.Name, account.MonthlyLimit);
            return Result<CategoryDTO>.Success(response);
        }
    }
}
