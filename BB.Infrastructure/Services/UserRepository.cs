using BB.Infrastructure.Data;
using BB.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BB.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<User>> GetAllAsync() =>
        await _context.Users.ToListAsync();

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FindAsync(id);

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateAsync(int id, User updated)
    {
        var existing = await _context.Users.FindAsync(id);
        if (existing is null) return null;

        existing.FirstName = updated.FirstName;
        existing.LastName  = updated.LastName;
        existing.Email     = updated.Email;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async  Task<User?> GetByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u=> u.Email == email);
        return user;
    }
}