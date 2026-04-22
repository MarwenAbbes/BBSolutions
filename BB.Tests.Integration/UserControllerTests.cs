using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BB.Domain.DTO;
using BB.Domain.Entities;
using BB.Infrastructure.Data;
using BB.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace BB.Tests.Integration;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly PasswordHasher passwordHasher;
    private readonly CreateUserRequest _newUser = new(){ FirstName = "Marwen", LastName = "Ben", Email = "marwen@bb.com", Password = "123456666" };
    private readonly LoginRequest _testUser = new()
    {
        Email = "admin@bb.com",
        Password = "Admin@123"
    };

    public UserControllerTests(CustomWebApplicationFactory factory)
    {
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
                Email = _testUser.Email,
                PasswordHash = HashPassword(_testUser.Password), // Use your actual password hashing method
                                                                 // Set other required properties
            });
            await context.SaveChangesAsync();
        }
    }
    private string HashPassword(string password)
    {
        return passwordHasher.Hash(password);
    }
    private async Task AuthenticateAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = _testUser.Email,    // from the factory
            Password = _testUser.Password // from the factory
        });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);
    }

    [Fact]
    public async Task GetAll_ShouldReturnPaginatedUserList()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/v1/user?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();
        var paginated = await response.Content.ReadFromJsonAsync<PaginatedResponse<UserResponse>>();
        Assert.NotNull(paginated);
        Assert.NotEmpty(paginated.Items);
        Assert.Equal(1, paginated.Page);
        Assert.Equal(10, paginated.PageSize);
        Assert.True(paginated.TotalCount > 0);
    }
    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/user");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task CreateUser_ShouldReturn201()
    {
        await AuthenticateAsync();
        var response = await _client.PostAsJsonAsync("/api/v1/user", _newUser);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(created);
        Assert.Equal("Marwen", created.FirstName);
    }
    [Fact]
    public async Task CreateUser_ResponseShouldNotContainPasswordHash()
    {
        await AuthenticateAsync();
        var response = await _client.PostAsJsonAsync("/api/v1/user", _newUser);
        var rawJson = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("passwordHash", rawJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", rawJson, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task GetUserById_ShouldReturn200()
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/user", _newUser);
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        var response = await _client.GetAsync($"/api/v1/user/{created!.Id}");
        response.EnsureSuccessStatusCode();
        var returned = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(returned);
        Assert.Equal(created.Id, returned.Id);
    }
    [Fact]
    public async Task UpdateUser_ShouldReturn200()
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/user", _newUser);
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        var updateRequest = new UpdateUserRequest
        {
            FirstName = "MarwenUpdated",
            LastName = _newUser.LastName,
            Email = _newUser.Email
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/user/{created!.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(updated);
        Assert.Equal("MarwenUpdated", updated.FirstName);
    }
    [Fact]
    public async Task DeleteUser_ShouldReturn204()
    {
        await AuthenticateAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/v1/user", _newUser);
        var created = await createResponse.Content.ReadFromJsonAsync<UserResponse>();
        var response = await _client.DeleteAsync($"/api/v1/user/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var getResponse = await _client.GetAsync($"/api/v1/user/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}