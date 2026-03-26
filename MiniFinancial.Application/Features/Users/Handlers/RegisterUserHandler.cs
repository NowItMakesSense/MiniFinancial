using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Users.Commands;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Users.Handlers
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<UserDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICurrentUserService _currentUser;

        public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher,
                                   IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, ICurrentUserService currentUser)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _currentUser = currentUser;
        }

        public async Task<Result<UserDTO>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var now = _dateTimeProvider.UtcNow;
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (!_currentUser.IsAdmin && request.Role == UserRole.Admin) throw new OwnershipViolationException("Você nao pode criar outro usuário Admin.");

            var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (existingUser is not null) throw new BusinessRuleException("Email já cadastrado.");

            var passwordHash = _passwordHasher.Hash(existingUser!, request.Password);
            var user = new User(request.Name, normalizedEmail, passwordHash, UserRole.User, now);

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UserDTO(user.Id, user.Name, user.Email);
            return Result<UserDTO>.Success(response);
        }
    }
}
