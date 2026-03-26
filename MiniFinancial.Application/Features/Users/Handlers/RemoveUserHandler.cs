using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Users.Commands;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Users.Handlers
{
    public class RemoveUserHandler : IRequestHandler<RemoveUserCommand, Result<UserDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public RemoveUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, ICurrentUserService currentUser)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public async Task<Result<UserDTO>> Handle(RemoveUserCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;

            var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
            if (existingUser is null) throw new BusinessRuleException("Usuario nao cadastrado.");
            if (!_currentUser.IsAdmin && existingUser.Id != _currentUser.UserId) throw new OwnershipViolationException("Você não pode alterar este usuário.");

            existingUser.Delete(now);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UserDTO(existingUser.Id, existingUser.Name, existingUser.Email);
            return Result<UserDTO>.Success(response);
        }
    }
}
