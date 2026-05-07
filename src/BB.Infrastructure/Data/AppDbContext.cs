using BB.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BB.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasQueryFilter(u => !u.IsDeleted);
            entity.Property(u => u.Email).HasMaxLength(450);
            entity.Property(u => u.RowVersion).IsRowVersion();
            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            var seedCreated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            entity.HasData(
                new
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@bb.com",
                    PasswordHash = "$2a$11$UZFNqhTwC8zKXyjFl4UG6O90kwygAyS6a6CMsv0C9c.l7c9Rjc0Sa",
                    CreatedAt = seedCreated,
                    IsDeleted = false,
                    EmailConfirmed = true,
                    EmailConfirmedAt = (DateTime?)seedCreated,
                    PasswordChangedAt = (DateTime?)seedCreated,
                    AccessFailedCount = 0,
                    LockoutEnd = (DateTimeOffset?)null,
                    LastLoginAt = (DateTime?)null
                });
        });
    }
}