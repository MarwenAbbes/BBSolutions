using BB.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BB.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@bb.com",
            PasswordHash = "$2a$11$UZFNqhTwC8zKXyjFl4UG6O90kwygAyS6a6CMsv0C9c.l7c9Rjc0Sa",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}