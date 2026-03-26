using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.Features.Transactions.Commands;
using MiniFinancial.Application.Features.Transactions.Handlers;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;
using Moq;

namespace MiniFinancial.Tests
{
    public class TransactionHandlersTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepository = new();
        private readonly Mock<IAccountRepository> _accountRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly Mock<ITransactionService> _transactionService = new();

        private DateTimeOffset Now => DateTimeOffset.UtcNow;

        #region Get
        [Fact]
        public async Task GetTransaction_Should_Return_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var account = new Account(user.Id, "Conta", true, Now);
            var transaction = new Transaction(
                account.Id,
                null,
                null,
                "Category",
                100,
                TransactionType.Expense,
                TransactionCategory.Debit,
                "desc",
                Now
            );

            _transactionRepository.Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
            _accountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new GetTransactionByIdHandler(_transactionRepository.Object,_accountRepository.Object,_currentUser.Object);
            var result = await handler.Handle(new GetTransactionByIdCommand(transaction.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(transaction.Id, result.Value!.Id);
        }

        [Fact]
        public async Task GetTransaction_Should_Throw_When_NotFound()
        {
            _transactionRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Transaction)null);

            var handler = new GetTransactionByIdHandler(_transactionRepository.Object, _accountRepository.Object, _currentUser.Object);

            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetTransactionByIdCommand(Guid.NewGuid()), CancellationToken.None));
        }
        #endregion

        #region Register
        [Fact]
        public async Task RegisterTransaction_Should_Create_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var account = new Account(user.Id, "Conta", true, Now);
            account.SetBalance(100, Now);
            var request = new RegisterTransactionCommand(
                account.Id,
                null,
                null,
                50,
                TransactionType.Expense,
                TransactionCategory.Debit,
                "desc"
            );

            var transaction = new Transaction(
                account.Id,
                null,
                null,
                "Category",
                50,
                TransactionType.Expense,
                TransactionCategory.Debit,
                "desc",
                Now
            );

            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);

            _accountRepository
                .Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            _transactionService.Setup(x =>
                x.CreateTransactionAsync(
                    It.IsAny<Account>(),
                    It.IsAny<Account?>(),
                    It.IsAny<RegisterTransactionCommand>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);

            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RegisterTransactionHandler(
                _transactionRepository.Object,
                _accountRepository.Object,
                _unitOfWork.Object,
                _dateTimeProvider.Object,
                _currentUser.Object,
                _transactionService.Object);

            var result = await handler.Handle(request, default);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task RegisterTransaction_Should_Throw_When_InsufficientBalance()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var account = new Account(user.Id, "Conta", false, Now);
            var request = new RegisterTransactionCommand(
                account.Id,
                null,
                null,
                999,
                TransactionType.Expense,
                TransactionCategory.Debit,
                "desc"
            );

            _accountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

            var handler = new RegisterTransactionHandler(_transactionRepository.Object, _accountRepository.Object, _unitOfWork.Object,
                                                         _dateTimeProvider.Object, _currentUser.Object, _transactionService.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(request, CancellationToken.None));
        }
        #endregion

        #region Update
        [Fact]
        public async Task UpdateTransaction_Should_Update_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var account = new Account(user.Id, "Conta", true, Now);
            var transaction = new Transaction(
                account.Id,
                null,
                null,
                "Category",
                100,
                TransactionType.Expense,
                TransactionCategory.Debit,
                "old",
                Now
            );

            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _transactionRepository.Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
            _accountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new UpdateTransactionHandler(_transactionRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object,
                                                       _currentUser.Object, _accountRepository.Object);

            var result = await handler.Handle(new UpdateTransactionCommand(transaction.Id, "new"), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("new", transaction.Description);

            _transactionRepository.Verify(x => x.Update(It.IsAny<Transaction>()), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region Remove

        [Fact]
        public async Task RemoveTransaction_Should_Create_Reversal_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var account = new Account(user.Id, "Conta", true, Now);
            var transaction = new Transaction(
                account.Id,
                null,
                null,
                "Category",
                100,
                TransactionType.Expense,
                TransactionCategory.Debit,
                "desc",
                Now
            );

            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _transactionRepository.Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
            _accountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RemoveTransactionHandler(_transactionRepository.Object, _accountRepository.Object, _unitOfWork.Object,
                                                       _dateTimeProvider.Object, _currentUser.Object);

            var result = await handler.Handle(new RemoveTransactionCommand(transaction.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);

            _transactionRepository.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveTransaction_Should_Throw_When_Income()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var transaction = new Transaction(
                Guid.NewGuid(),
                null,
                null,
                "Category",
                100,
                TransactionType.Income,
                TransactionCategory.Debit,
                "desc",
                Now
            );

            _transactionRepository.Setup(x => x.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);

            var handler = new RemoveTransactionHandler(_transactionRepository.Object, _accountRepository.Object, _unitOfWork.Object,
                                                       _dateTimeProvider.Object, _currentUser.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RemoveTransactionCommand(transaction.Id), CancellationToken.None));
        }
        #endregion
    }
}
