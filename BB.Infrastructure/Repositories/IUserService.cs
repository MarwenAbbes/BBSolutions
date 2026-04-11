using BB.Infrastructure.Models;

namespace BB.Infrastructure.Repositories;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(int id, User updated);
    Task<bool> DeleteAsync(int id);
}