using System.Security.Cryptography;
using System.Text;
using CodeSync.Events.Version;
using CodeSync.ReviewHub.DTOs;
using CodeSync.ReviewHub.Models;
using CodeSync.ReviewHub.Repositories;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using MassTransit;

namespace CodeSync.ReviewHub.Services
{
    public class SnapshotServiceImpl : ISnapshotService
    {
        private readonly ISnapshotRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<SnapshotServiceImpl> _logger;

        public SnapshotServiceImpl(
            ISnapshotRepository repo,
            IPublishEndpoint publishEndpoint,
            ILogger<SnapshotServiceImpl> logger)
        {
            _repo            = repo;
            _publishEndpoint = publishEndpoint;
            _logger          = logger;
        }

        // ── Create Snapshot ───────────────────────────────────────────────────

        public async Task<SnapshotResponse> CreateSnapshotAsync(
            int authorId, CreateSnapshotRequest req)
        {
            // Compute SHA-256 hash of content
            var hash = ComputeSha256(req.Content);

            // Find parent snapshot
            var parent = await _repo.FindLatestByFileIdAsync(
                req.FileId, req.Branch);

            var snapshot = new Snapshot
            {
                ProjectId        = req.ProjectId,
                FileId           = req.FileId,
                AuthorId         = authorId,
                Message          = req.Message,
                Content          = req.Content,
                Hash             = hash,
                Branch           = req.Branch,
                ParentSnapshotId = parent?.SnapshotId
            };

            await _repo.CreateAsync(snapshot);

            // ✅ SAGA — publish SnapshotCreated event
            // Notification-Service listens → notifies collaborators
            await _publishEndpoint.Publish(new SnapshotCreated(
                SnapshotId: snapshot.SnapshotId,
                ProjectId:  snapshot.ProjectId,
                FileId:     snapshot.FileId,
                AuthorId:   authorId,
                Message:    snapshot.Message,
                Branch:     snapshot.Branch,
                CreatedAt:  snapshot.CreatedAt
            ));

            _logger.LogInformation(
                "SnapshotCreated published for SnapshotId={SnapshotId}",
                snapshot.SnapshotId);

            return MapToResponse(snapshot);
        }

        // ── Get Snapshots ─────────────────────────────────────────────────────

        public async Task<SnapshotResponse> GetSnapshotByIdAsync(int snapshotId)
        {
            var snapshot = await _repo.FindBySnapshotIdAsync(snapshotId)
                ?? throw new KeyNotFoundException(
                    $"Snapshot {snapshotId} not found.");
            return MapToResponse(snapshot);
        }

        public async Task<IEnumerable<SnapshotResponse>> GetSnapshotsByFileAsync(
            int fileId)
        {
            var snapshots = await _repo.FindByFileIdAsync(fileId);
            return snapshots.Select(MapToResponse);
        }

        public async Task<IEnumerable<SnapshotResponse>> GetSnapshotsByProjectAsync(
            int projectId)
        {
            var snapshots = await _repo.FindByProjectIdAsync(projectId);
            return snapshots.Select(MapToResponse);
        }

        public async Task<IEnumerable<SnapshotResponse>> GetSnapshotsByBranchAsync(
            int projectId, string branch)
        {
            var snapshots = await _repo.FindByBranchAsync(projectId, branch);
            return snapshots.Select(MapToResponse);
        }

        public async Task<SnapshotResponse?> GetLatestSnapshotAsync(
            int fileId, string branch)
        {
            var snapshot = await _repo.FindLatestByFileIdAsync(fileId, branch);
            return snapshot is null ? null : MapToResponse(snapshot);
        }

        // ── Restore Snapshot ──────────────────────────────────────────────────

        public async Task<SnapshotResponse> RestoreSnapshotAsync(
            int snapshotId, int requestingUserId)
        {
            var original = await _repo.FindBySnapshotIdAsync(snapshotId)
                ?? throw new KeyNotFoundException(
                    $"Snapshot {snapshotId} not found.");

            // Verify integrity
            var computedHash = ComputeSha256(original.Content);
            if (computedHash != original.Hash)
                throw new InvalidOperationException(
                    "Snapshot integrity check failed — content may be corrupted.");

            // Non-destructive restore — create new snapshot with old content
            var parent = await _repo.FindLatestByFileIdAsync(
                original.FileId, original.Branch);

            var restored = new Snapshot
            {
                ProjectId        = original.ProjectId,
                FileId           = original.FileId,
                AuthorId         = requestingUserId,
                Message          = $"Restored from snapshot #{snapshotId}: {original.Message}",
                Content          = original.Content,
                Hash             = original.Hash,
                Branch           = original.Branch,
                ParentSnapshotId = parent?.SnapshotId
            };

            await _repo.CreateAsync(restored);

            // ✅ SAGA — publish FileRestored event
            await _publishEndpoint.Publish(new FileRestored(
                FileId:          original.FileId,
                ProjectId:       original.ProjectId,
                SnapshotId:      snapshotId,
                RestoredByUserId: requestingUserId,
                RestoredAt:      DateTime.UtcNow
            ));

            _logger.LogInformation(
                "FileRestored published for FileId={FileId} SnapshotId={SnapshotId}",
                original.FileId, snapshotId);

            return MapToResponse(restored);
        }

        // ── Diff Snapshots ────────────────────────────────────────────────────

        public async Task<DiffResponse> DiffSnapshotsAsync(
            int snapshotId1, int snapshotId2)
        {
            var snap1 = await _repo.FindBySnapshotIdAsync(snapshotId1)
                ?? throw new KeyNotFoundException(
                    $"Snapshot {snapshotId1} not found.");

            var snap2 = await _repo.FindBySnapshotIdAsync(snapshotId2)
                ?? throw new KeyNotFoundException(
                    $"Snapshot {snapshotId2} not found.");

            // Use DiffPlex Myers algorithm
            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diff        = diffBuilder.BuildDiffModel(snap1.Content, snap2.Content);

            var linesAdded     = diff.Lines.Count(l => l.Type == ChangeType.Inserted);
            var linesRemoved   = diff.Lines.Count(l => l.Type == ChangeType.Deleted);
            var linesUnchanged = diff.Lines.Count(l => l.Type == ChangeType.Unchanged);

            // Build readable diff result
            var diffResult = new StringBuilder();
            foreach (var line in diff.Lines)
            {
                var prefix = line.Type switch
                {
                    ChangeType.Inserted  => "+ ",
                    ChangeType.Deleted   => "- ",
                    ChangeType.Modified  => "~ ",
                    _                    => "  "
                };
                diffResult.AppendLine($"{prefix}{line.Text}");
            }

            return new DiffResponse
            {
                SnapshotId1    = snapshotId1,
                SnapshotId2    = snapshotId2,
                DiffResult     = diffResult.ToString(),
                LinesAdded     = linesAdded,
                LinesRemoved   = linesRemoved,
                LinesUnchanged = linesUnchanged
            };
        }

        // ── Tag Snapshot ──────────────────────────────────────────────────────

        public async Task TagSnapshotAsync(
            int snapshotId, TagSnapshotRequest request)
        {
            var snapshot = await _repo.FindBySnapshotIdAsync(snapshotId)
                ?? throw new KeyNotFoundException(
                    $"Snapshot {snapshotId} not found.");

            snapshot.Tag = request.Tag;
            await _repo.UpdateAsync(snapshot);
        }

        // ── Get Branches ──────────────────────────────────────────────────────

        public async Task<IEnumerable<string>> GetBranchesAsync(int projectId)
        {
            var snapshots = await _repo.FindByProjectIdAsync(projectId);
            return snapshots.Select(s => s.Branch).Distinct().ToList();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string ComputeSha256(string content)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            return Convert.ToHexString(bytes).ToLower();
        }

        private static SnapshotResponse MapToResponse(Snapshot s) => new()
        {
            SnapshotId       = s.SnapshotId,
            ProjectId        = s.ProjectId,
            FileId           = s.FileId,
            AuthorId         = s.AuthorId,
            Message          = s.Message,
            Hash             = s.Hash,
            Branch           = s.Branch,
            Tag              = s.Tag,
            ParentSnapshotId = s.ParentSnapshotId,
            IsArchived       = s.IsArchived,
            CreatedAt        = s.CreatedAt
        };

        private readonly StringBuilder diffResult = new();
    }
}