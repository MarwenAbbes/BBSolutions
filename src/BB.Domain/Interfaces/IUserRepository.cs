using BB.Domain.Entities;

namespace BB.Domain.Interfaces;

public interface IUserRepository
{
    Task<(IEnumerable<User> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(int id, User updated);
    Task<bool> DeleteAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task RecordLoginFailureAsync(User user, CancellationToken cancellationToken = default);
    Task RecordLoginSuccessAsync(User user, CancellationToken cancellationToken = default);
}
