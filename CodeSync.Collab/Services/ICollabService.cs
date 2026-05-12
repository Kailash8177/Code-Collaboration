using CodeSync.Collab.DTOs;

namespace CodeSync.Collab.Services
{
    public interface ICollabService
    {
        Task<SessionResponse> CreateSessionAsync(
            int ownerId, CreateSessionRequest request);
        Task<SessionResponse> GetSessionByIdAsync(Guid sessionId);
        Task<IEnumerable<SessionResponse>> GetSessionsByProjectAsync(
            int projectId);
        Task<IEnumerable<SessionResponse>> GetActiveSessionsByProjectAsync(
            int projectId);
        Task<SessionResponse?> GetActiveSessionByFileAsync(int fileId);
        Task<ParticipantResponse> JoinSessionAsync(
            int userId, JoinSessionRequest request);
        Task LeaveSessionAsync(Guid sessionId, int userId);
        Task EndSessionAsync(Guid sessionId, int requestingUserId);
        Task KickParticipantAsync(
            Guid sessionId, int requestingUserId, int targetUserId);
        Task<IEnumerable<ParticipantResponse>> GetParticipantsAsync(
            Guid sessionId);
        Task UpdateCursorAsync(
            Guid sessionId, int userId, int line, int col);
        Task UpdateLastActivityAsync(Guid sessionId);
    }
}