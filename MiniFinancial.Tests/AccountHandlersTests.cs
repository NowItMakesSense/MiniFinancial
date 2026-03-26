using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.Features.Accounts.Commads;
using MiniFinancial.Application.Features.Accounts.Handlers;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;
using Moq;

namespace MiniFinancial.Tests
{
    public class AccountHandlersTests
    {
        private readonly Mock<IAccountRepository> _accountRepository = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();

        private DateTimeOffset Now => DateTimeOffset.UtcNow;

        #region Get Account
        [Fact]
        public async Task GetAccount_Should_Return_When_User_Is_Owner()
        {
            var userId = Guid.NewGuid();
            var account = new Account(userId, "Conta", false, Now);

            _accountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

            _currentUser.Setup(x => x.IsAdmin).Returns(false);
            _currentUser.Setup(x => x.UserId).Returns(userId);

            var handler = new GetAccountByIdHandler(_accountRepository.Object, _currentUser.Object);
            var result = await handler.Handle(new GetAccountByIdCommand(account.Id), default);

            Assert.True(result.IsSuccess);
            Assert.Equal(account.Id, result.Value!.Id);
        }

        [Fact]
        public async Task GetAccount_Should_Throw_When_NotFound()
        {
            _accountRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account)null);

            var handler = new GetAccountByIdHandler(_accountRepository.Object, _currentUser.Object);

            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetAccountByIdCommand(Guid.NewGuid()), default));
        }

        [Fact]
        public async Task GetAccount_Should_Throw_When_NotOwner()
        {
            var account = new Account(Guid.NewGuid(), "Conta", false, Now);

            _accountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

            _currentUser.Setup(x => x.IsAdmin).Returns(false);
            _currentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var handler = new GetAccountByIdHandler(_accountRepository.Object, _currentUser.Object);

            await Assert.ThrowsAsync<OwnershipViolationException>(() => handler.Handle(new GetAccountByIdCommand(account.Id), default));
        }
        #endregion

        #region Register
        [Fact]
        public async Task RegisterAccount_Should_Create_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);

            _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _accountRepository.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Account)null);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RegisterAccountHandler(_accountRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object,
                                                     _userRepository.Object, _currentUser.Object);

            var result = await handler.Handle(new RegisterAccountCommand(user.Id, "Conta"), default);

            Assert.True(result.IsSuccess);

            _accountRepository.Verify(x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAccount_Should_Throw_When_User_NotFound()
        {
            _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RegisterAccountHandler(_accountRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object,
                                                     _userRepository.Object, _currentUser.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RegisterAccountCommand(Guid.NewGuid(), "Conta"), default));
        }

        [Fact]
        public async Task RegisterAccount_Should_Throw_When_AlreadyExists()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var account = new Account(user.Id, "Conta", false, Now);

            _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _accountRepository.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RegisterAccountHandler(_accountRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object,
                                                     _userRepository.Object, _currentUser.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RegisterAccountCommand(user.Id, "Conta"), default));
        }
        #endregion

        #region Update
        [Fact]
        public async Task UpdateAccount_Should_Update_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var account = new Account(user.Id, "Conta", false, Now);

            _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _accountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new UpdateAccountHandler(_accountRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);
            var result = await handler.Handle(new UpdateAccountCommand(account.Id, "Nova Conta"), default);

            Assert.True(result.IsSuccess);
            Assert.Equal("Nova Conta", account.Name);
        }

        [Fact]
        public async Task UpdateAccount_Should_Throw_When_NotFound()
        {
            _accountRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account)null);

            var handler = new UpdateAccountHandler(_accountRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new UpdateAccountCommand(Guid.NewGuid(), "Nome"), default));
        }
        #endregion

        #region Remove
        [Fact]
        public async Task RemoveAccount_Should_Delete_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var account = new Account(user.Id, "Conta", false, Now);

            _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _accountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RemoveAccountHandler(_accountRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);
            var result = await handler.Handle(new RemoveAccountCommand(account.Id), default);

            Assert.True(result.IsSuccess);

            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveAccount_Should_Throw_When_NotFound()
        {
            _accountRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account)null);

            var handler = new RemoveAccountHandler(_accountRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RemoveAccountCommand(Guid.NewGuid()), default));
        }
        #endregion
    }
}