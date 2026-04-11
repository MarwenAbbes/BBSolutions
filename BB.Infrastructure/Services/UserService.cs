using BB.Infrastructure.Models;
using BB.Infrastructure.Security;
using Microsoft.Extensions.Logging;

namespace BB.Infrastructure.Repositories;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly ILogger<UserService> _logger;
    private readonly IPasswordHasher _passwordHasher;
    public UserService(IUserRepository repo, ILogger<UserService> logger, IPasswordHasher passwordHasher)
    {
        _repo = repo;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = await _repo.GetAllAsync();
        _logger.LogDebug("Get all users. Count {}", users.Count());
        return users;
    } 
    public async Task<User?> GetByIdAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        _logger.LogDebug("Get user with id {}", id);
        return user;
    }

    public async Task<User> CreateAsync(User user)
    {
        user.PasswordHash = _passwordHasher.Hash(user.PasswordHash);
        var created = await _repo.CreateAsync(user);
        _logger.LogDebug("User created with ID {Id}", created.Id);
        return created;
    }

    public async Task<User?> UpdateAsync(int id, User updated)
    {
        var user = await _repo.UpdateAsync(id, updated);
        _logger.LogDebug("User updated with ID {Id}", updated.Id);
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
       var result = await _repo.DeleteAsync(id);
       if(result)
           _logger.LogDebug("User with ID {} deleted",id);
       return result;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var result = await _repo.GetByEmailAsync(email);
        _logger.LogDebug("User Id {} by Email {}",result.Id, email);
        return result;
    }
}