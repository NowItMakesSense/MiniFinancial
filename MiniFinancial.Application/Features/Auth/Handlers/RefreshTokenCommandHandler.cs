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
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;

        public RefreshTokenCommandHandler(IUserRefreshTokenRepository refreshTokenRepository, ITokenService tokenService, IUnitOfWork unitOfWork)
        {
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LoginDTO>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (storedToken is null) throw new BusinessRuleException("Token Invalido.");

            if (!storedToken.IsActive())
            {
                // reuse attack detectado
                await _refreshTokenRepository.RevokeAllByUserIdAsync(storedToken.UserId, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                throw new BusinessRuleException("Token sendo reutilizado.");
            }

            var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
            storedToken.Revoke(newRefreshTokenValue);

            var newRefreshToken = new UserRefreshToken(
                storedToken.UserId,
                newRefreshTokenValue,
                DateTime.UtcNow.AddDays(7)
            );

            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new LoginDTO(newRefreshTokenValue, null, DateTimeOffset.UtcNow);
            return Result<LoginDTO>.Success(response);
        }
    }
}
