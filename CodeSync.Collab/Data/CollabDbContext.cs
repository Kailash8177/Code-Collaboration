using CodeSync.Collab.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Collab.Data
{
    public class CollabDbContext : DbContext
    {
        public CollabDbContext(DbContextOptions<CollabDbContext> options)
            : base(options) { }

        public DbSet<CollabSession> CollabSessions => Set<CollabSession>();
        public DbSet<Participant> Participants => Set<Participant>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CollabSession>(e =>
            {
                e.HasKey(s => s.SessionId);

                e.Property(s => s.Status)
                 .IsRequired()
                 .HasMaxLength(20)
                 .HasDefaultValue("ACTIVE");

                e.Property(s => s.Language)
                 .HasMaxLength(50);

                e.Property(s => s.MaxParticipants)
                 .HasDefaultValue(10);

                e.Property(s => s.IsPasswordProtected)
                 .HasDefaultValue(false);

                e.Property(s => s.CreatedAt)
                 .HasDefaultValueSql("NOW()");

                e.Property(s => s.LastActivityAt)
                 .HasDefaultValueSql("NOW()");

                e.HasMany(s => s.Participants)
                 .WithOne(p => p.Session)
                 .HasForeignKey(p => p.SessionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Participant>(e =>
            {
                e.HasKey(p => p.ParticipantId);

                e.HasIndex(p => new { p.SessionId, p.UserId })
                 .IsUnique();

                e.Property(p => p.Role)
                 .IsRequired()
                 .HasMaxLength(20)
                 .HasDefaultValue("EDITOR");

                e.Property(p => p.Color)
                 .HasMaxLength(20)
                 .HasDefaultValue("#FF5733");

                e.Property(p => p.IsActive)
                 .HasDefaultValue(true);

                e.Property(p => p.JoinedAt)
                 .HasDefaultValueSql("NOW()");
            });
        }
    }
}