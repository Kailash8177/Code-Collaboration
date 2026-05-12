using CodeSync.File.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.File.Data
{
    public class FileDbContext : DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options)
            : base(options) { }

        public DbSet<CodeFile> CodeFiles => Set<CodeFile>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CodeFile>(e =>
            {
                e.HasKey(f => f.FileId);

                e.HasIndex(f => new { f.ProjectId, f.Path })
                 .IsUnique()
                 .HasFilter("\"IsDeleted\" = false");

                e.Property(f => f.Name)
                 .IsRequired()
                 .HasMaxLength(255);

                e.Property(f => f.Path)
                 .IsRequired()
                 .HasMaxLength(1000);

                e.Property(f => f.Language)
                 .HasMaxLength(50);

                e.Property(f => f.Content)
                 .HasColumnType("text");

                e.Property(f => f.IsDeleted)
                 .HasDefaultValue(false);

                e.Property(f => f.IsFolder)
                 .HasDefaultValue(false);

                e.Property(f => f.Size)
                 .HasDefaultValue(0L);

                e.Property(f => f.CreatedAt)
                 .HasDefaultValueSql("NOW()");

                e.Property(f => f.UpdatedAt)
                 .HasDefaultValueSql("NOW()");
            });
        }
    }
}