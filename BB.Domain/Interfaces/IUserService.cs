using BB.Domain.Entities;

namespace BB.Domain.Interfaces;

public interface IUserService
{
    Task<(IEnumerable<User> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(int id, User updated);
    Task<bool> DeleteAsync(int id);
}
