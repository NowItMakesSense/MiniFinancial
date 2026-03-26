using MiniFinancial.Application.Contracts.Interfaces;
using MiniFinancial.Application.Contracts.Repositories;
using MiniFinancial.Application.Features.Users.Commands;
using MiniFinancial.Application.Features.Users.Handlers;
using MiniFinancial.Domain.Entities;
using MiniFinancial.Domain.Enums;
using MiniFinancial.Domain.Exceptions;
using Moq;

public class UserAuthorizationTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private DateTimeOffset Now => DateTimeOffset.UtcNow;

    #region REGISTER
    [Fact]
    public async Task User_Should_Create_User_Role()
    {
        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<object>(), It.IsAny<string>())).Returns("valid-hash");

        var handler = new RegisterUserHandler(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object,
                                              _dateTimeProvider.Object, _currentUser.Object);

        var result = await handler.Handle(new RegisterUserCommand("John", "email", "123", UserRole.User), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task User_Should_Not_Create_Admin_When_Not_Admin()
    {
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        var handler = new RegisterUserHandler(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object,
                                              _dateTimeProvider.Object, _currentUser.Object);

        await Assert.ThrowsAsync<OwnershipViolationException>(() => handler.Handle(new RegisterUserCommand("John", "email", "123", UserRole.Admin), CancellationToken.None));
    }

    [Fact]
    public async Task Admin_Should_Create_Admin()
    {
        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<object>(), It.IsAny<string>())).Returns("valid-hash");

        var handler = new RegisterUserHandler(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object,
                                              _dateTimeProvider.Object, _currentUser.Object);

        var result = await handler.Handle(new RegisterUserCommand("John", "email", "123", UserRole.Admin), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }
    #endregion

    #region GET
    [Fact]
    public async Task User_Should_Get_Self()
    {
        var user = new User("John", "email", "hash", UserRole.User, Now);

        _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _currentUser.Setup(x => x.UserId).Returns(user.Id);
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        var handler = new GetUserByIdQueryHandler(_userRepository.Object, _currentUser.Object);
        var result = await handler.Handle(new GetUserByIdCommand(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task User_Should_Not_Get_Other_User()
    {
        var user = new User("John", "email", "hash", UserRole.User, Now);

        _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _currentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        var handler = new GetUserByIdQueryHandler(_userRepository.Object, _currentUser.Object);
        await Assert.ThrowsAsync<OwnershipViolationException>(() => handler.Handle(new GetUserByIdCommand(user.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Admin_Should_Get_Other_User()
    {
        var user = new User("John", "email", "hash", UserRole.User, Now);

        _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        var handler = new GetUserByIdQueryHandler(_userRepository.Object, _currentUser.Object);
        var result = await handler.Handle(new GetUserByIdCommand(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
    #endregion

    #region UPDATE
    [Fact]
    public async Task User_Should_Update_Self()
    {
        var user = new User("Old", "email", "hash", UserRole.User, Now);

        _userRepository.Setup(x => x.GetByEmailAsync("email", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _currentUser.Setup(x => x.UserId).Returns(user.Id);
        _currentUser.Setup(x => x.IsAdmin).Returns(false);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<object>(), It.IsAny<string>())).Returns("valid-hash");

        var handler = new UpdateUserHandler(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object,
                                            _dateTimeProvider.Object, _currentUser.Object);

        await handler.Handle(new UpdateUserCommand(user.Id, "New", "email", "123", UserRole.User), CancellationToken.None);
        Assert.Equal("New", user.Name);
    }

    [Fact]
    public async Task User_Should_Not_Update_Other_User()
    {
        var user = new User("Old", "email", "hash", UserRole.User, Now);

        _userRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _currentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        var handler = new UpdateUserHandler(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object,
                                            _dateTimeProvider.Object, _currentUser.Object);

        await Assert.ThrowsAsync<OwnershipViolationException>(() => handler.Handle(new UpdateUserCommand(Guid.NewGuid(), "New", "email", null, UserRole.User), CancellationToken.None));
    }

    [Fact]
    public async Task Admin_Should_Update_Other_User()
    {
        var user = new User("Old", "email", "hash", UserRole.User, Now);

        _userRepository.Setup(x => x.GetByEmailAsync("email", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _currentUser.Setup(x => x.IsAdmin).Returns(true);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<object>(), It.IsAny<string>())).Returns("valid-hash");

        var handler = new UpdateUserHandler(_userRepository.Object, _passwordHasher.Object, _unitOfWork.Object,
                                            _dateTimeProvider.Object, _currentUser.Object);

        await handler.Handle(new UpdateUserCommand(user.Id, "New", "email", "123", UserRole.Admin), CancellationToken.None);

        Assert.Equal("New", user.Name);
        Assert.Equal(UserRole.Admin, user.Role);
    }
    #endregion

    #region DELETE
    [Fact]
    public async Task User_Should_Delete_Self()
    {
        var user = new User("John", "email", "hash", UserRole.User, Now);

        _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _currentUser.Setup(x => x.UserId).Returns(user.Id);
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        var handler = new RemoveUserHandler(_userRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);
        await handler.Handle(new RemoveUserCommand(user.Id), CancellationToken.None);

        Assert.True(user.IsDeleted);
    }

    [Fact]
    public async Task User_Should_Not_Delete_Other_User()
    {
        var user = new User("John", "email", "hash", UserRole.User, Now);

        _userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _currentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());
        _currentUser.Setup(x => x.IsAdmin).Returns(false);

        var handler = new RemoveUserHandler(_userRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);
        await Assert.ThrowsAsync<OwnershipViolationException>(() => handler.Handle(new RemoveUserCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Admin_Should_Delete_Other_User()
    {
        var user = new User("John", "email", "hash", UserRole.User, Now);

        _userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _currentUser.Setup(x => x.IsAdmin).Returns(true);

        var handler = new RemoveUserHandler(_userRepository.Object, _unitOfWork.Object, _dateTimeProvider.Object, _currentUser.Object);
        await handler.Handle(new RemoveUserCommand(user.Id), CancellationToken.None);

        Assert.True(user.IsDeleted);
    }
    #endregion
}