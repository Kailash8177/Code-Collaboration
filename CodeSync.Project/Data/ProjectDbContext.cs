using CodeSync.Project.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Project.Data
{
    public class ProjectDbContext : DbContext
    {
        public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
            : base(options) { }

        public DbSet<Models.Project> Projects => Set<Models.Project>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<ProjectStar> ProjectStars => Set<ProjectStar>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Models.Project>(e =>
            {
                e.HasKey(p => p.ProjectId);

                e.HasIndex(p => new { p.OwnerId, p.Name });

                e.Property(p => p.Name)
                 .IsRequired()
                 .HasMaxLength(100);

                e.Property(p => p.Description)
                 .HasMaxLength(500);

                e.Property(p => p.Language)
                 .IsRequired()
                 .HasMaxLength(50);

                e.Property(p => p.Visibility)
                 .IsRequired()
                 .HasMaxLength(20)
                 .HasDefaultValue("PUBLIC");

                e.Property(p => p.IsArchived)
                 .HasDefaultValue(false);

                e.Property(p => p.StarCount)
                 .HasDefaultValue(0);

                e.Property(p => p.ForkCount)
                 .HasDefaultValue(0);

                e.Property(p => p.CreatedAt)
                 .HasDefaultValueSql("NOW()");

                e.Property(p => p.UpdatedAt)
                 .HasDefaultValueSql("NOW()");

                e.HasMany(p => p.Members)
                 .WithOne(m => m.Project)
                 .HasForeignKey(m => m.ProjectId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(p => p.Stars)
                 .WithOne(s => s.Project)
                 .HasForeignKey(s => s.ProjectId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ProjectMember>(e =>
            {
                e.HasKey(m => m.Id);

                e.HasIndex(m => new { m.ProjectId, m.UserId })
                 .IsUnique();

                e.Property(m => m.Role)
                 .IsRequired()
                 .HasMaxLength(20)
                 .HasDefaultValue("MEMBER");

                e.Property(m => m.JoinedAt)
                 .HasDefaultValueSql("NOW()");
            });

            builder.Entity<ProjectStar>(e =>
            {
                e.HasKey(s => s.Id);

                e.HasIndex(s => new { s.ProjectId, s.UserId })
                 .IsUnique();

                e.Property(s => s.StarredAt)
                 .HasDefaultValueSql("NOW()");
            });
        }
    }
}