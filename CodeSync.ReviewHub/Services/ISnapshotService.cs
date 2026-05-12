using CodeSync.ReviewHub.DTOs;

namespace CodeSync.ReviewHub.Services
{
    public interface ISnapshotService
    {
        Task<SnapshotResponse> CreateSnapshotAsync(
            int authorId, CreateSnapshotRequest request);
        Task<SnapshotResponse> GetSnapshotByIdAsync(int snapshotId);
        Task<IEnumerable<SnapshotResponse>> GetSnapshotsByFileAsync(int fileId);
        Task<IEnumerable<SnapshotResponse>> GetSnapshotsByProjectAsync(
            int projectId);
        Task<IEnumerable<SnapshotResponse>> GetSnapshotsByBranchAsync(
            int projectId, string branch);
        Task<SnapshotResponse?> GetLatestSnapshotAsync(int fileId, string branch);
        Task<SnapshotResponse> RestoreSnapshotAsync(
            int snapshotId, int requestingUserId);
        Task<DiffResponse> DiffSnapshotsAsync(int snapshotId1, int snapshotId2);
        Task TagSnapshotAsync(int snapshotId, TagSnapshotRequest request);
        Task<IEnumerable<string>> GetBranchesAsync(int projectId);
    }
}