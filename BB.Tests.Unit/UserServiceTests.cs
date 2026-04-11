using BB.Infrastructure.Models;
using BB.Infrastructure.Repositories;
using BB.Infrastructure.Security;
using Microsoft.Extensions.Logging;
using Moq;

namespace BB.Tests.Unit;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly Mock<IPasswordHasher>  _mockHasher;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _service;
    
    private readonly List<User> fakeUsers = new List<User>
    {
        new User { Id = 1, FirstName = "Marwen", LastName = "Ben", Email = "marwen@bb.com", PasswordHash = "123456789"},
        new User { Id = 2, FirstName = "John",   LastName = "Doe", Email = "john@bb.com", PasswordHash = "123456789" }
    };
    public UserServiceTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _mockHasher = new Mock<IPasswordHasher>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _service  = new UserService(_mockRepo.Object, _mockLogger.Object,_mockHasher.Object);
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(fakeUsers);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserAsync_ShouldReturnUser()
    {
        //Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(fakeUsers[1]);

        // Act
        var result = await _service.GetByIdAsync(2);
        
        //Assert
        Assert.Equal(2, result.Id);
        _mockRepo.Verify(r => r.GetByIdAsync(2), Times.Once);
    }

    [Fact]
    public async Task GetUserAsync_ShouldReturnNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((User?)null);
        var result = await _service.GetByIdAsync(2);
        Assert.Null(result);
        _mockRepo.Verify(r => r.GetByIdAsync(2), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUser()
    {
      
        _mockRepo.Setup(r => r.CreateAsync(fakeUsers[0])).ReturnsAsync(fakeUsers[0]);
        var result = await _service.CreateAsync(fakeUsers[0]);
        Assert.NotNull(result);
        Assert.Equal(fakeUsers[0].Id, result.Id);
        _mockRepo.Verify(r => r.CreateAsync(result), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUser()
    {
        var fakeUser = fakeUsers[0];
        var UpdatedFakeUser = new User { Id = 1, FirstName = "Mario", LastName = "Ben", Email = "marwen@bb.com", PasswordHash = "123456789" };

        _mockRepo.Setup(r=> r.UpdateAsync(fakeUser.Id, UpdatedFakeUser)).ReturnsAsync(UpdatedFakeUser);
        
        var result = await _service.UpdateAsync(fakeUser.Id, UpdatedFakeUser);
        Assert.NotNull(result);
        Assert.Equal(UpdatedFakeUser.FirstName, result.FirstName);
        Assert.Equal(UpdatedFakeUser.LastName, result.LastName);
        Assert.Equal(UpdatedFakeUser.Email, result.Email);
        _mockRepo.Verify(r => r.UpdateAsync(fakeUser.Id, UpdatedFakeUser), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUser()
    {
      

        _mockRepo.Setup(r=> r.DeleteAsync(fakeUsers[0].Id)).ReturnsAsync(true);
        
        var result = await _service.DeleteAsync(fakeUsers[0].Id);
        Assert.True(result);
        _mockRepo.Verify(r => r.DeleteAsync(fakeUsers[0].Id), Times.Once);
    }
}