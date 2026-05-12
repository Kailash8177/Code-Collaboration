using CodeSync.ReviewHub.Models;

namespace CodeSync.ReviewHub.Repositories
{
    public interface ISnapshotRepository
    {
        Task<Snapshot?> FindBySnapshotIdAsync(int snapshotId);
        Task<IEnumerable<Snapshot>> FindByFileIdAsync(int fileId);
        Task<IEnumerable<Snapshot>> FindByProjectIdAsync(int projectId);
        Task<IEnumerable<Snapshot>> FindByBranchAsync(int projectId, string branch);
        Task<IEnumerable<Snapshot>> FindByAuthorIdAsync(int authorId);
        Task<Snapshot?> FindByHashAsync(string hash);
        Task<Snapshot?> FindByTagAsync(string tag);
        Task<Snapshot?> FindLatestByFileIdAsync(int fileId, string branch);
        Task<Snapshot> CreateAsync(Snapshot snapshot);
        Task<Snapshot> UpdateAsync(Snapshot snapshot);
        Task ArchiveByFileIdAsync(int fileId);
        Task ArchiveByProjectIdAsync(int projectId);
    }
}