using CodeSync.ReviewHub.Data;
using CodeSync.ReviewHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ReviewHub.Repositories
{
    public class SnapshotRepository : ISnapshotRepository
    {
        private readonly ReviewHubDbContext _db;

        public SnapshotRepository(ReviewHubDbContext db)
        {
            _db = db;
        }

        public async Task<Snapshot?> FindBySnapshotIdAsync(int snapshotId) =>
            await _db.Snapshots.FindAsync(snapshotId);

        public async Task<IEnumerable<Snapshot>> FindByFileIdAsync(int fileId) =>
            await _db.Snapshots
                .Where(s => s.FileId == fileId && !s.IsArchived)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Snapshot>> FindByProjectIdAsync(
            int projectId) =>
            await _db.Snapshots
                .Where(s => s.ProjectId == projectId && !s.IsArchived)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Snapshot>> FindByBranchAsync(
            int projectId, string branch) =>
            await _db.Snapshots
                .Where(s => s.ProjectId == projectId
                    && s.Branch == branch
                    && !s.IsArchived)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Snapshot>> FindByAuthorIdAsync(
            int authorId) =>
            await _db.Snapshots
                .Where(s => s.AuthorId == authorId)
                .ToListAsync();

        public async Task<Snapshot?> FindByHashAsync(string hash) =>
            await _db.Snapshots
                .FirstOrDefaultAsync(s => s.Hash == hash);

        public async Task<Snapshot?> FindByTagAsync(string tag) =>
            await _db.Snapshots
                .FirstOrDefaultAsync(s => s.Tag == tag);

        public async Task<Snapshot?> FindLatestByFileIdAsync(
            int fileId, string branch) =>
            await _db.Snapshots
                .Where(s => s.FileId == fileId
                    && s.Branch == branch
                    && !s.IsArchived)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

        public async Task<Snapshot> CreateAsync(Snapshot snapshot)
        {
            _db.Snapshots.Add(snapshot);
            await _db.SaveChangesAsync();
            return snapshot;
        }

        public async Task<Snapshot> UpdateAsync(Snapshot snapshot)
        {
            _db.Snapshots.Update(snapshot);
            await _db.SaveChangesAsync();
            return snapshot;
        }

        public async Task ArchiveByFileIdAsync(int fileId)
        {
            var snapshots = await _db.Snapshots
                .Where(s => s.FileId == fileId && !s.IsArchived)
                .ToListAsync();

            foreach (var s in snapshots)
                s.IsArchived = true;

            await _db.SaveChangesAsync();
        }

        public async Task ArchiveByProjectIdAsync(int projectId)
        {
            var snapshots = await _db.Snapshots
                .Where(s => s.ProjectId == projectId && !s.IsArchived)
                .ToListAsync();

            foreach (var s in snapshots)
                s.IsArchived = true;

            await _db.SaveChangesAsync();
        }
    }
}