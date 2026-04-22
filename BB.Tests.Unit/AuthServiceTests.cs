using BB.Domain.DTO;
using BB.Domain.Entities;
using BB.Domain.Interfaces;
using BB.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BB.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly Mock<IPasswordHasher> _mockHasher;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly AuthService _service;

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
        _mockRepo = new Mock<IUserRepository>();
        _mockHasher = new Mock<IPasswordHasher>();
        _mockConfig = new Mock<IConfiguration>();

        _mockConfig.Setup(c => c["Jwt:Secret"])
            .Returns("test-secret-key-that-is-at-least-32-characters-long");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("BB.API");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("BB.API");
        _mockConfig.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

        _service = new AuthService(_mockRepo.Object, _mockHasher.Object, _mockConfig.Object);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByEmailAsync("unknown@bb.com"))
            .ReturnsAsync((User?)null);

        var result = await _service.LoginAsync(new LoginRequest
        {
            Email = "unknown@bb.com",
            Password = "anypassword"
        });

        Assert.Null(result);
        _mockHasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByEmailAsync("admin@bb.com"))
            .ReturnsAsync(_fakeUser);
        _mockHasher.Setup(h => h.Verify("wrongpassword", _fakeUser.PasswordHash))
            .Returns(false);

        var result = await _service.LoginAsync(new LoginRequest
        {
            Email = "admin@bb.com",
            Password = "wrongpassword"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        _mockRepo.Setup(r => r.GetByEmailAsync("admin@bb.com"))
            .ReturnsAsync(_fakeUser);
        _mockHasher.Setup(h => h.Verify("Admin@123", _fakeUser.PasswordHash))
            .Returns(true);

        var result = await _service.LoginAsync(new LoginRequest
        {
            Email = "admin@bb.com",
            Password = "Admin@123"
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.True(result.ExpiresAt < DateTime.UtcNow.AddMinutes(61));
    }
}