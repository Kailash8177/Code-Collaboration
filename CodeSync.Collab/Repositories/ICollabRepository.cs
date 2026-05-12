using CodeSync.Collab.Models;

namespace CodeSync.Collab.Repositories
{
    public interface ICollabRepository
    {
        // ── Sessions ──────────────────────────────────────────────────────────
        Task<CollabSession?> FindBySessionIdAsync(Guid sessionId);
        Task<IEnumerable<CollabSession>> FindByProjectIdAsync(int projectId);
        Task<IEnumerable<CollabSession>> FindByFileIdAsync(int fileId);
        Task<IEnumerable<CollabSession>> FindActiveByProjectIdAsync(int projectId);
        Task<IEnumerable<CollabSession>> FindByOwnerIdAsync(int ownerId);
        Task<CollabSession?> FindActiveByFileIdAsync(int fileId);
        Task<CollabSession> CreateAsync(CollabSession session);
        Task<CollabSession> UpdateAsync(CollabSession session);

        // ── Participants ──────────────────────────────────────────────────────
        Task<IEnumerable<Participant>> FindParticipantsBySessionIdAsync(
            Guid sessionId);
        Task<Participant?> FindParticipantAsync(Guid sessionId, int userId);
        Task<int> CountParticipantsAsync(Guid sessionId);
        Task<Participant> AddParticipantAsync(Participant participant);
        Task<Participant> UpdateParticipantAsync(Participant participant);
    }
}