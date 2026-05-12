using CodeSync.Events.Projects;
using CodeSync.File.Models;
using CodeSync.File.Repositories;
using MassTransit;

namespace CodeSync.File.Consumers
{
    // ✅ SAGA Consumer
    // Listens for ProjectCreated event
    // Creates default README.md in every new project
    public class ProjectCreatedConsumer : IConsumer<ProjectCreated>
    {
        private readonly IFileRepository _fileRepo;
        private readonly ILogger<ProjectCreatedConsumer> _logger;

        public ProjectCreatedConsumer(
            IFileRepository fileRepo,
            ILogger<ProjectCreatedConsumer> logger)
        {
            _fileRepo = fileRepo;
            _logger   = logger;
        }

        public async Task Consume(ConsumeContext<ProjectCreated> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "ProjectCreated received — creating README for ProjectId={ProjectId}",
                evt.ProjectId);

            try
            {
                var readme = new CodeFile
                {
                    ProjectId    = evt.ProjectId,
                    Name         = "README.md",
                    Path         = "README.md",
                    Language     = "markdown",
                    Content      = $"# {evt.Name}\n\n"
                                 + $"Created on {evt.CreatedAt:yyyy-MM-dd}\n\n"
                                 + $"Language: {evt.Language}\n\n"
                                 + $"## Getting Started\n\n"
                                 + $"Add your project description here.",
                    Size         = 0,
                    CreatedById  = evt.OwnerId,
                    LastEditedBy = evt.OwnerId,
                    IsFolder     = false
                };

                await _fileRepo.CreateAsync(readme);

                _logger.LogInformation(
                    "README.md created for ProjectId={ProjectId}",
                    evt.ProjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create README for ProjectId={ProjectId}",
                    evt.ProjectId);
                // Throw so MassTransit retries
                throw;
            }
        }
    }
}