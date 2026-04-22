using BB.Domain.DTO;
using BB.Domain.Entities;
using BB.Domain.Interfaces;
using BB.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BB.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly AuthService _authService;
    private readonly Mock<IConfiguration> _mockConfig;

    private readonly User _fakeUser = new()
    {
        Id = 1,
        FirstName = "Admin",
        LastName = "User",
        Email = "admin@bb.com",
        PasswordHash = "$2a$11$hashedpasswordplaceholder"
    };

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _mockConfig = new Mock<IConfiguration>();

        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns("test-secret-key-that-is-at-least-32-characters-long");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("BB.API");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("BB.API");
        _mockConfig.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

        _authService = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _mockConfig.Object);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnNull()
    {
        //Arrange
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync("unkown@bb.com")).ReturnsAsync((User?)null);
        // Act
        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "unkown@bb.com",
            Password = "any-password"
        });

        // Assert
        Assert.Null(result);
        _passwordHasherMock.Verify(ph => ph.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithIncorrectPassword_ReturnNull()
    {
        // Arrange
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync("admin@bb.com")).ReturnsAsync(_fakeUser);
        _passwordHasherMock.Setup(ph => ph.Verify("wrongpassword", _fakeUser.PasswordHash)).Returns(false);

        // Act
        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "admin@bb.com",
            Password = "wrongpassword"
        });

        // Assert
        Assert.Null(result);
        _passwordHasherMock.Verify(ph => ph.Verify("wrongpassword", _fakeUser.PasswordHash), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithCorrectCredentials_ReturnToken()
    {
        // Arrange
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync("admin@bb.com")).ReturnsAsync(_fakeUser);
        _passwordHasherMock.Setup(ph => ph.Verify("correctpassword", _fakeUser.PasswordHash)).Returns(true);

        // Act
        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "admin@bb.com",
            Password = "correctpassword"
        });

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.True(result.ExpiresAt <= DateTime.UtcNow.AddHours(1));
        _passwordHasherMock.Verify(ph => ph.Verify("correctpassword", _fakeUser.PasswordHash), Times.Once);
    }
}