using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Auth.Commands;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Auth.Handlers
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IUserRefreshTokenRepository _refreshTokenRepository;

        public LoginUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork,
                                        ITokenService tokenService, IUserRefreshTokenRepository refreshTokenRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<Result<LoginDTO>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null) throw new BusinessRuleException("Credenciais invalidas.");

            var isValidPassword = _passwordHasher.Verify(user, request.Password, user.PasswordHash);
            if (!isValidPassword) throw new BusinessRuleException("Credenciais invalidas.");

            var accessToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshToken = new UserRefreshToken(user.Id, refreshTokenValue, DateTimeOffset.UtcNow.AddDays(7));

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var currentUser = new UserDTO(user.Id, user.Name, user.Email);
            var response = new LoginDTO(accessToken, currentUser, DateTimeOffset.UtcNow);
            return Result<LoginDTO>.Success(response);
        }
    }
}
