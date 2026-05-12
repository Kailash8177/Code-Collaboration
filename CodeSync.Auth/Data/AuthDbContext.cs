using CodeSync.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Auth.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshTokenEntry> RefreshTokens => Set<RefreshTokenEntry>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(e =>
            {
                e.HasKey(u => u.UserId);

                e.HasIndex(u => u.Email)
                 .IsUnique();

                e.HasIndex(u => u.Username)
                 .IsUnique();

                e.Property(u => u.Username)
                 .IsRequired()
                 .HasMaxLength(30);

                e.Property(u => u.Email)
                 .IsRequired()
                 .HasMaxLength(100);

                e.Property(u => u.Role)
                 .IsRequired()
                 .HasMaxLength(20)
                 .HasDefaultValue("DEVELOPER");

                e.Property(u => u.Provider)
                 .IsRequired()
                 .HasMaxLength(20)
                 .HasDefaultValue("LOCAL");

                e.Property(u => u.AvatarUrl)
                 .HasMaxLength(500);

                e.Property(u => u.Bio)
                 .HasMaxLength(300);

                e.Property(u => u.IsActive)
                 .HasDefaultValue(true);

                e.Property(u => u.CreatedAt)
                 .HasDefaultValueSql("NOW()");
            });

            builder.Entity<RefreshTokenEntry>(e =>
            {
                e.HasKey(r => r.Id);

                e.HasIndex(r => r.Token)
                 .IsUnique();

                e.Property(r => r.Token)
                 .IsRequired();

                e.Property(r => r.IsRevoked)
                 .HasDefaultValue(false);

                e.Property(r => r.CreatedAt)
                 .HasDefaultValueSql("NOW()");
            });
        }
    }
}