using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Users.Commands;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Users.Handlers
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public UpdateUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork,
                                 IDateTimeProvider dateTimeProvider, ICurrentUserService currentUser)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public async Task<Result<UserDTO>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (existingUser is null) throw new BusinessRuleException("Usuario não encontrado.");
            if (!_currentUser.IsAdmin && existingUser.Id != _currentUser.UserId) throw new OwnershipViolationException("Você não pode alterar este usuário.");

            existingUser.UpdateProfile(request.Name, now);
            existingUser.ChangeRole(request.Role, now);

            var hash = _passwordHasher.Hash(existingUser, request.Password);
            existingUser.ChangePassword(hash, now);

            _userRepository.Update(existingUser);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UserDTO(existingUser.Id, existingUser.Name, existingUser.Email);
            return Result<UserDTO>.Success(response);
        }
    }
}
