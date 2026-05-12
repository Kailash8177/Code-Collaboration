using CodeSync.Execution.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Execution.Data
{
    public class ExecutionDbContext : DbContext
    {
        public ExecutionDbContext(DbContextOptions<ExecutionDbContext> options)
            : base(options) { }

        public DbSet<ExecutionJob> ExecutionJobs => Set<ExecutionJob>();
        public DbSet<SupportedLanguage> SupportedLanguages
            => Set<SupportedLanguage>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ExecutionJob>(e =>
            {
                e.HasKey(j => j.JobId);

                e.HasIndex(j => j.UserId);
                e.HasIndex(j => j.Status);
                e.HasIndex(j => j.Language);

                e.Property(j => j.Language)
                 .IsRequired()
                 .HasMaxLength(50);

                e.Property(j => j.Status)
                 .IsRequired()
                 .HasMaxLength(20)
                 .HasDefaultValue("QUEUED");

                e.Property(j => j.SourceCode)
                 .HasColumnType("text");

                e.Property(j => j.Stdout)
                 .HasColumnType("text");

                e.Property(j => j.Stderr)
                 .HasColumnType("text");

                e.Property(j => j.CreatedAt)
                 .HasDefaultValueSql("NOW()");
            });

            builder.Entity<SupportedLanguage>(e =>
            {
                e.HasKey(l => l.Id);

                e.Property(l => l.Name)
                 .IsRequired()
                 .HasMaxLength(50);

                e.Property(l => l.Version)
                 .IsRequired()
                 .HasMaxLength(50);

                e.Property(l => l.IsEnabled)
                 .HasDefaultValue(true);

                // Seed supported languages
                e.HasData(
                    new SupportedLanguage
                    {
                        Id = 1,
                        Name = "python",
                        Version = "3.11",
                        FileExtension = ".py",
                        RunCommand = "python3 {file}",
                        IsEnabled = true
                    },
                    new SupportedLanguage
                    {
                        Id = 2,
                        Name = "javascript",
                        Version = "Node.js 20",
                        FileExtension = ".js",
                        RunCommand = "node {file}",
                        IsEnabled = true
                    },
                    new SupportedLanguage
                    {
                        Id = 3,
                        Name = "java",
                        Version = "21",
                        FileExtension = ".java",
                        RunCommand = "java {file}",
                        IsEnabled = true
                    },
                    new SupportedLanguage
                    {
                        Id = 4,
                        Name = "csharp",
                        Version = ".NET 8",
                        FileExtension = ".cs",
                        RunCommand = "dotnet script {file}",
                        IsEnabled = true
                    }
                );
            });
        }
    }
}