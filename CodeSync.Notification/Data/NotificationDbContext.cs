using CodeSync.Notification.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Notification.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(
            DbContextOptions<NotificationDbContext> options)
            : base(options) { }

        public DbSet<Models.Notification> Notifications
            => Set<Models.Notification>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Models.Notification>(e =>
            {
                e.HasKey(n => n.NotificationId);

                e.HasIndex(n => n.RecipientId);
                e.HasIndex(n => n.IsRead);
                e.HasIndex(n => n.Type);

                e.Property(n => n.Type)
                 .IsRequired()
                 .HasMaxLength(50);

                e.Property(n => n.Title)
                 .IsRequired()
                 .HasMaxLength(200);

                e.Property(n => n.Message)
                 .IsRequired()
                 .HasMaxLength(1000);

                e.Property(n => n.RelatedId)
                 .HasMaxLength(100);

                e.Property(n => n.RelatedType)
                 .HasMaxLength(50);

                e.Property(n => n.IsRead)
                 .HasDefaultValue(false);

                e.Property(n => n.CreatedAt)
                 .HasDefaultValueSql("NOW()");
            });
        }
    }
}