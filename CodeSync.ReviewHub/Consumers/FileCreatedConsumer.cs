using CodeSync.Events.Files;
using CodeSync.ReviewHub.Models;
using CodeSync.ReviewHub.Repositories;
using MassTransit;
using System.Security.Cryptography;
using System.Text;

namespace CodeSync.ReviewHub.Consumers
{
    // ✅ SAGA Consumer
    // When file is created → create initial empty snapshot
    public class FileCreatedConsumer : IConsumer<FileCreated>
    {
        private readonly ISnapshotRepository _repo;
        private readonly ILogger<FileCreatedConsumer> _logger;

        public FileCreatedConsumer(
            ISnapshotRepository repo,
            ILogger<FileCreatedConsumer> logger)
        {
            _repo   = repo;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<FileCreated> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "FileCreated received — creating initial snapshot for FileId={FileId}",
                evt.FileId);

            var content = string.Empty;
            var hash    = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLower();

            var snapshot = new Snapshot
            {
                ProjectId = evt.ProjectId,
                FileId    = evt.FileId,
                AuthorId  = evt.CreatedById,
                Message   = "Initial commit",
                Content   = content,
                Hash      = hash,
                Branch    = "main"
            };

            await _repo.CreateAsync(snapshot);

            _logger.LogInformation(
                "Initial snapshot created for FileId={FileId}", evt.FileId);
        }
    }
}