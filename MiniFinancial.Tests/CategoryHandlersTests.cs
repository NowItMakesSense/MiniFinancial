using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.Features.Categories.Commands;
using MiniFinancial.Application.Features.Categories.Handlers;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;
using Moq;

namespace MiniFinancial.Tests
{
    public class CategoryHandlersTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepository = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();

        private DateTimeOffset Now => DateTimeOffset.UtcNow;

        #region Get
        [Fact]
        public async Task GetCategory_Should_Return_When_User_Is_Owner()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var category = new Category(user.Id, "Food", 100, Now);

            _categoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _currentUser.Setup(x => x.IsAdmin).Returns(false);
            _currentUser.Setup(x => x.UserId).Returns(user.Id);

            var handler = new GetCategoryByIdHandler(_categoryRepository.Object, _currentUser.Object);
            var result = await handler.Handle(new GetCategoryByIdCommand(category.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(category.Id, result.Value!.Id);
        }

        [Fact]
        public async Task GetCategory_Should_Throw_When_NotFound()
        {
            _categoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Category)null);

            var handler = new GetCategoryByIdHandler(_categoryRepository.Object, _currentUser.Object);

            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetCategoryByIdCommand(Guid.NewGuid()), CancellationToken.None));
        }

        [Fact]
        public async Task GetCategory_Should_Throw_When_NotOwner()
        {
            var category = new Category(Guid.NewGuid(), "Food", 100, Now);

            _categoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _currentUser.Setup(x => x.IsAdmin).Returns(false);
            _currentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var handler = new GetCategoryByIdHandler(_categoryRepository.Object, _currentUser.Object);

            await Assert.ThrowsAsync<OwnershipViolationException>(() => handler.Handle(new GetCategoryByIdCommand(category.Id), CancellationToken.None));
        }
        #endregion

        #region Register
        [Fact]
        public async Task RegisterCategory_Should_Create_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);

            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _categoryRepository.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Category>());
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RegisterCategoryHandler(_categoryRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object,
                                                      _userRepository.Object, _currentUser.Object);

            var result = await handler.Handle(new RegisterCategoryCommand(user.Id, "Food", 200), CancellationToken.None);

            Assert.True(result.IsSuccess);

            _categoryRepository.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterCategory_Should_Throw_When_User_NotFound()
        {
            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RegisterCategoryHandler(_categoryRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object,
                                                      _userRepository.Object, _currentUser.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RegisterCategoryCommand(Guid.NewGuid(), "Food", 100), CancellationToken.None));
        }

        [Fact]
        public async Task RegisterCategory_Should_Throw_When_Duplicate_Name()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);

            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);

            var existingCategories = new List<Category>{
                new Category(user.Id, "Food", 100, Now)
            };

            _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _categoryRepository.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingCategories);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RegisterCategoryHandler(_categoryRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object,
                                                      _userRepository.Object, _currentUser.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RegisterCategoryCommand(user.Id, "Food", 200), CancellationToken.None));
        }
        #endregion

        #region Update

        [Fact]
        public async Task UpdateCategory_Should_Update_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var category = new Category(user.Id, "Food", 100, Now);

            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _categoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new UpdateCategoryHandler(_categoryRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);
            var result = await handler.Handle(new UpdateCategoryCommand(category.Id, "New Food", 300), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("New Food", category.Name);
            Assert.Equal(300, category.MonthlyLimit);
        }

        [Fact]
        public async Task UpdateCategory_Should_Throw_When_NotFound()
        {
            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _categoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Category)null);

            var handler = new UpdateCategoryHandler(_categoryRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new UpdateCategoryCommand(Guid.NewGuid(), "Name", 100), CancellationToken.None));
        }
        #endregion

        #region Remove
        [Fact]
        public async Task RemoveCategory_Should_Delete_When_Valid()
        {
            var user = new User("John", "email", "hash", UserRole.User, Now);
            var category = new Category(user.Id, "Food", 100, Now);

            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _categoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
            _currentUser.Setup(x => x.IsAdmin).Returns(true);

            var handler = new RemoveCategoryHandler(_categoryRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);
            var result = await handler.Handle(new RemoveCategoryCommand(category.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);

            _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveCategory_Should_Throw_When_NotFound()
        {
            _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
            _categoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Category)null);

            var handler = new RemoveCategoryHandler(_categoryRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);

            await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new RemoveCategoryCommand(Guid.NewGuid()), CancellationToken.None));
        }
        #endregion
    }
}