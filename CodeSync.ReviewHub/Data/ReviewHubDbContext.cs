using CodeSync.ReviewHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ReviewHub.Data
{
    public class ReviewHubDbContext : DbContext
    {
        public ReviewHubDbContext(DbContextOptions<ReviewHubDbContext> options)
            : base(options) { }

        public DbSet<Snapshot> Snapshots => Set<Snapshot>();
        public DbSet<Comment> Comments => Set<Comment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // ── Snapshot ──────────────────────────────────────────────────────
            builder.Entity<Snapshot>(e =>
            {
                e.HasKey(s => s.SnapshotId);

                e.HasIndex(s => s.FileId);
                e.HasIndex(s => s.ProjectId);
                e.HasIndex(s => s.Branch);
                e.HasIndex(s => s.Hash);

                e.Property(s => s.Message)
                 .IsRequired()
                 .HasMaxLength(500);

                e.Property(s => s.Hash)
                 .IsRequired()
                 .HasMaxLength(64);

                e.Property(s => s.Branch)
                 .IsRequired()
                 .HasMaxLength(100)
                 .HasDefaultValue("main");

                e.Property(s => s.Tag)
                 .HasMaxLength(50);

                e.Property(s => s.Content)
                 .HasColumnType("text");

                e.Property(s => s.IsArchived)
                 .HasDefaultValue(false);

                e.Property(s => s.CreatedAt)
                 .HasDefaultValueSql("NOW()");
            });

            // ── Comment ───────────────────────────────────────────────────────
            builder.Entity<Comment>(e =>
            {
                e.HasKey(c => c.CommentId);

                e.HasIndex(c => c.FileId);
                e.HasIndex(c => c.ProjectId);
                e.HasIndex(c => c.AuthorId);

                e.Property(c => c.Content)
                 .IsRequired()
                 .HasMaxLength(2000);

                e.Property(c => c.IsResolved)
                 .HasDefaultValue(false);

                e.Property(c => c.ColumnNumber)
                 .HasDefaultValue(0);

                e.Property(c => c.CreatedAt)
                 .HasDefaultValueSql("NOW()");

                e.Property(c => c.UpdatedAt)
                 .HasDefaultValueSql("NOW()");
            });
        }
    }
}