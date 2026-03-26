using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.Features.Auth.Commands;
using MiniFinancial.Application.Features.Auth.Handlers;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;
using Moq;

namespace MiniFinancial.Tests
{
    public class AuthHandlersTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IUserRefreshTokenRepository> _refreshTokenRepository = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private DateTimeOffset Now => DateTimeOffset.UtcNow;

        #region LOGIN
        [Fact]
        public async Task Login_Should_Succeed_When_Credentials_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);

            _userRepository.Setup(x => x.GetByEmailAsync("email", It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _passwordHasher.Setup(x => x.Verify(user, "123456", "hash")).Returns(true);
            _jwtTokenGenerator.Setup(x => x.GenerateToken(user.Id, user.Email, user.Role)).Returns("access-token");
            _tokenService.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");

            var handler = new LoginUserCommandHandler(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object,
                                                      _tokenService.Object, _refreshTokenRepository.Object, _jwtTokenGenerator.Object);

            var result = await handler.Handle(new LoginUserCommand("email", "123456"), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            _refreshTokenRepository.Verify(x => x.AddAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Login_Should_Throw_When_User_Not_Found()
        {
            _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null);

            var handler = new LoginUserCommandHandler(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object,
                                                      _tokenService.Object, _refreshTokenRepository.Object, _jwtTokenGenerator.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new LoginUserCommand("email", "123"), CancellationToken.None));
        }

        [Fact]
        public async Task Login_Should_Throw_When_Invalid_Password()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);

            _userRepository.Setup(x => x.GetByEmailAsync("email", It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _passwordHasher.Setup(x => x.Verify(user, It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var handler = new LoginUserCommandHandler(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object,
                                                      _tokenService.Object, _refreshTokenRepository.Object, _jwtTokenGenerator.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new LoginUserCommand("email", "wrong"), CancellationToken.None));
        }
        #endregion

        #region LOGOUT
        [Fact]
        public async Task Logout_Should_Revoke_All_User_Tokens()
        {
            var token = new UserRefreshToken(Guid.NewGuid(), "token", Now.AddDays(1));

            _refreshTokenRepository.Setup(x => x.GetByTokenAsync("token", It.IsAny<CancellationToken>())).ReturnsAsync(token);

            var handler = new LogoutCommandHandler(_refreshTokenRepository.Object, _unitOfWork.Object);

            var result = await handler.Handle(new LogoutCommand("token"), CancellationToken.None);

            Assert.True(result.IsSuccess);

            _refreshTokenRepository.Verify(x => x.RevokeAllByUserIdAsync(token.UserId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Logout_Should_Throw_When_Token_Invalid()
        {
            _refreshTokenRepository.Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserRefreshToken)null);

            var handler = new LogoutCommandHandler(_refreshTokenRepository.Object, _unitOfWork.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new LogoutCommand("token"), CancellationToken.None));
        }
        #endregion

        #region REFRESH
        [Fact]
        public async Task Refresh_Should_Generate_New_Token_When_Valid()
        {
            var token = new UserRefreshToken(Guid.NewGuid(), "token", Now.AddDays(1));

            _refreshTokenRepository.Setup(x => x.GetByTokenAsync("token", It.IsAny<CancellationToken>())).ReturnsAsync(token);
            _tokenService.Setup(x => x.GenerateRefreshToken()).Returns("new-refresh");

            var handler = new RefreshTokenCommandHandler(_refreshTokenRepository.Object, _tokenService.Object, _unitOfWork.Object);

            var result = await handler.Handle(new RefreshTokenCommand("token"), CancellationToken.None);

            Assert.True(result.IsSuccess);

            _refreshTokenRepository.Verify(x => x.AddAsync(It.IsAny<UserRefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Refresh_Should_Throw_When_Token_Invalid()
        {
            _refreshTokenRepository.Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserRefreshToken)null);

            var handler = new RefreshTokenCommandHandler(_refreshTokenRepository.Object, _tokenService.Object, _unitOfWork.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RefreshTokenCommand("token"), CancellationToken.None));
        }

        [Fact]
        public async Task Refresh_Should_Revoke_All_When_Reused_Token()
        {
            var expiredToken = new UserRefreshToken(Guid.NewGuid(), "token", Now.AddDays(-1));

            _refreshTokenRepository.Setup(x => x.GetByTokenAsync("token", It.IsAny<CancellationToken>())).ReturnsAsync(expiredToken);

            var handler = new RefreshTokenCommandHandler(_refreshTokenRepository.Object, _tokenService.Object, _unitOfWork.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RefreshTokenCommand("token"), CancellationToken.None));

            _refreshTokenRepository.Verify(x => x.RevokeAllByUserIdAsync(expiredToken.UserId, It.IsAny<CancellationToken>()), Times.Once);
        }
        #endregion

        #region REVOKE
        [Fact]
        public async Task Revoke_Should_Revoke_Token()
        {
            var token = new UserRefreshToken(Guid.NewGuid(), "token", Now.AddDays(1));

            _refreshTokenRepository.Setup(x => x.GetByTokenAsync("token", It.IsAny<CancellationToken>())).ReturnsAsync(token);

            var handler = new RevokeRefreshTokenCommandHandler(_refreshTokenRepository.Object, _unitOfWork.Object);

            var result = await handler.Handle(new RevokeRefreshTokenCommand("token"), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(token.IsRevoked);

            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Revoke_Should_Throw_When_Token_Not_Found()
        {
            _refreshTokenRepository.Setup(x => x.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserRefreshToken)null);

            var handler = new RevokeRefreshTokenCommandHandler(_refreshTokenRepository.Object, _unitOfWork.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RevokeRefreshTokenCommand("token"), CancellationToken.None));
        }
        #endregion
    }
}