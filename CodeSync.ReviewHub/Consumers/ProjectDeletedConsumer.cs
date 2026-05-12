using CodeSync.Events.Projects;
using CodeSync.ReviewHub.Repositories;
using MassTransit;

namespace CodeSync.ReviewHub.Consumers
{
    // ✅ SAGA Consumer
    // When project is deleted → archive all snapshots and delete all comments
    public class ProjectDeletedConsumer : IConsumer<ProjectDeleted>
    {
        private readonly ISnapshotRepository _snapshotRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly ILogger<ProjectDeletedConsumer> _logger;

        public ProjectDeletedConsumer(
            ISnapshotRepository snapshotRepo,
            ICommentRepository commentRepo,
            ILogger<ProjectDeletedConsumer> logger)
        {
            _snapshotRepo = snapshotRepo;
            _commentRepo  = commentRepo;
            _logger       = logger;
        }

        public async Task Consume(ConsumeContext<ProjectDeleted> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "ProjectDeleted received — cleaning up for ProjectId={ProjectId}",
                evt.ProjectId);

            await _snapshotRepo.ArchiveByProjectIdAsync(evt.ProjectId);
            await _commentRepo.DeleteByProjectIdAsync(evt.ProjectId);

            _logger.LogInformation(
                "Snapshots and comments cleaned for ProjectId={ProjectId}",
                evt.ProjectId);
        }
    }
}