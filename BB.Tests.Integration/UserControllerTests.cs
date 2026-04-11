using System.Net;
using System.Net.Http.Json;
using BB.Infrastructure.Models;

namespace BB.Tests.Integration;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly  User newUser = new User { FirstName = "Marwen", LastName = "Ben", Email = "marwen@bb.com",  PasswordHash = "123456" };
    
    public UserControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnUserList()
    {
        var response = await _client.GetAsync("/api/v1/user");
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }
    
    [Fact]
    public async Task CreateUser_ShouldReturn201()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/user", newUser);
    
        var created = await response.Content.ReadFromJsonAsync<User>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("Marwen", created.FirstName);
    }

    [Fact]
    public async Task GetUserById_ShouldReturn200()
    {
      
        var createResponse = await _client.PostAsJsonAsync("/api/v1/user", newUser);
        var created = await createResponse.Content.ReadFromJsonAsync<User>();
        
        
        // Then fetch it by ID
        var response = await _client.GetAsync($"/api/v1/user/{created!.Id}");
        response.EnsureSuccessStatusCode();
        var returned = await response.Content.ReadFromJsonAsync<User>();

        Assert.NotNull(returned);
        Assert.Equal(created.Id, returned.Id);
        Assert.Equal("Marwen", returned.FirstName);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturn200()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/user", newUser);
        var created = await createResponse.Content.ReadFromJsonAsync<User>();

        created.FirstName += "Updated";
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/user/{created.Id}", created);
        var updated = await updateResponse.Content.ReadFromJsonAsync<User>();
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal(newUser.FirstName+"Updated", updated.FirstName);
    }
    
    [Fact]
    public async Task GetAllUsers_ShouldReturn200()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/user", newUser);
        
        var response = await _client.GetAsync("/api/v1/user");
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturn204()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/v1/user", newUser);
        var created = await createResponse.Content.ReadFromJsonAsync<User>();

        var response = await _client.DeleteAsync($"/api/v1/User/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify user no longer exists
        var getResponse = await _client.GetAsync($"/api/v1/user/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}