using CodeSync.Events.Files;
using CodeSync.ReviewHub.Repositories;
using MassTransit;

namespace CodeSync.ReviewHub.Consumers
{
    // ✅ SAGA Consumer
    // When file is deleted → archive all snapshots
    public class FileDeletedConsumer : IConsumer<FileDeleted>
    {
        private readonly ISnapshotRepository _repo;
        private readonly ILogger<FileDeletedConsumer> _logger;

        public FileDeletedConsumer(
            ISnapshotRepository repo,
            ILogger<FileDeletedConsumer> logger)
        {
            _repo   = repo;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FileDeleted> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "FileDeleted received — archiving snapshots for FileId={FileId}",
                evt.FileId);

            await _repo.ArchiveByFileIdAsync(evt.FileId);

            _logger.LogInformation(
                "Snapshots archived for FileId={FileId}", evt.FileId);
        }
    }
}