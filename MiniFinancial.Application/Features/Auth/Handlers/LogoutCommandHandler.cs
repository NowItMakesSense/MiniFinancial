using MediatR;
using MiniFinancial.Application.Common;
using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.DTOs;
using MiniFinancial.Application.Features.Auth.Commands;
using MiniFinancial.Domain.Exceptions;

namespace MiniFinancial.Application.Features.Auth.Handlers
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<LoginDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRefreshTokenRepository _refreshTokenRepository;

        public LogoutCommandHandler(IUserRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LoginDTO>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            if (token is null) throw new BusinessRuleException("Token Invalido.");

            token.Revoke();
            await _refreshTokenRepository.RevokeAllByUserIdAsync(token.UserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new LoginDTO("", null, DateTimeOffset.UtcNow);
            return Result<LoginDTO>.Success(response);
        }
    }
}
