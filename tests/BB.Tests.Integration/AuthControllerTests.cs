using System.Net;
using System.Net.Http.Json;
using BB.Domain.DTO;
using BB.Domain.Entities;
using BB.Infrastructure.Data;
using BB.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace BB.Tests.Integration
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly PasswordHasher passwordHasher;
        private readonly LoginRequest _testUser = new()
        {
            Email = "admin@bb.com",
            Password = "Admin@123"
        };

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            passwordHasher = new();
            SeedTestUser(factory).GetAwaiter().GetResult();
        }

        // Ensure the test user exists in the test database
        private async Task SeedTestUser(CustomWebApplicationFactory factory)
        {
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!context.Users.Any(u => u.Email == _testUser.Email))
            {
                context.Users.Add(new User
                {
                    FirstName = "Admin",
                    LastName = "Test",
                    Email = _testUser.Email,
                    PasswordHash = HashPassword(_testUser.Password)
                });
                await context.SaveChangesAsync();
            }
        }
        private string HashPassword(string password)
        {
            return passwordHasher.Hash(password);
        }

        private async Task<(string Email, string Password)> SeedLockoutProbeUserAsync()
        {
            var email = $"lockout_{Guid.NewGuid():N}@bb.com";
            const string password = "LockoutProbe@123";
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Users.Add(new User
            {
                FirstName = "Lockout",
                LastName = "Probe",
                Email = email,
                PasswordHash = HashPassword(password)
            });
            await context.SaveChangesAsync();
            return (email, password);
        }
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsTokenAnd200()
        {
            //Act --Login
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", _testUser);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
            Assert.NotNull(content);
            Assert.False(string.IsNullOrEmpty(content.Token));
            Assert.True(content.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginWithWrongPassword_Returns401()
        {
            //Act --Login
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
            {
                Email = _testUser.Email,
                Password = "WrongPassword"
            });

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_AfterMaxFailedAttempts_LockoutBlocksEvenCorrectPassword()
        {
            var (email, password) = await SeedLockoutProbeUserAsync();
            for (var i = 0; i < 5; i++)
            {
                var fail = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
                {
                    Email = email,
                    Password = "WrongPassword"
                });
                Assert.Equal(HttpStatusCode.Unauthorized, fail.StatusCode);
            }

            var locked = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
            {
                Email = email,
                Password = password
            });
            Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
        }

        [Fact]
        public async Task LoginWithNonExistentEmail_Return401()
        {
            //Act --Login
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
            {
                Email = "non_existent@bb.com",
                Password = "SomePassword"
            });

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidEmailFormat_Returns400()
        {
            //Act --Login
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
            {
                Email = "invalid-email-format",
                Password = "SomePassword"
            });
            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithEmptyPassword_Returns400()
        {
            //Act --Login
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
            {
                Email = _testUser.Email,
                Password = ""
            });
            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithoutToken_Returns401()
        {
            var response = await _client.GetAsync("/api/v1/user");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        [Fact]
        public async Task ProtectedEndpoint_WithValidToken_Returns200()
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
            {
                Email = _testUser.Email,
                Password = _testUser.Password
            });
            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
            // Act -- use token to access protected endpoint
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/user");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);
            var response = await _client.SendAsync(request);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
