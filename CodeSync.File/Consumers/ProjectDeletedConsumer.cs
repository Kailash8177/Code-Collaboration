using CodeSync.Events.Projects;
using CodeSync.File.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.File.Consumers
{
    // ✅ SAGA Consumer
    // Listens for ProjectDeleted event
    // Soft deletes ALL files in the deleted project
    // This is the COMPENSATING ACTION in the Saga
    public class ProjectDeletedConsumer : IConsumer<ProjectDeleted>
    {
        private readonly FileDbContext _db;
        private readonly ILogger<ProjectDeletedConsumer> _logger;

        public ProjectDeletedConsumer(
            FileDbContext db,
            ILogger<ProjectDeletedConsumer> logger)
        {
            _db     = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProjectDeleted> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "ProjectDeleted received — soft deleting files for ProjectId={ProjectId}",
                evt.ProjectId);

            // Soft delete all files in this project
            var files = await _db.CodeFiles
                .Where(f => f.ProjectId == evt.ProjectId && !f.IsDeleted)
                .ToListAsync();

            foreach (var file in files)
            {
                file.IsDeleted = true;
                file.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Soft deleted {Count} files for ProjectId={ProjectId}",
                files.Count, evt.ProjectId);
        }
    }
}