using CodeSync.Collab.Data;
using CodeSync.Collab.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.Collab.Repositories
{
    public class CollabRepository : ICollabRepository
    {
        private readonly CollabDbContext _db;

        public CollabRepository(CollabDbContext db)
        {
            _db = db;
        }

        public async Task<CollabSession?> FindBySessionIdAsync(Guid sessionId) =>
            await _db.CollabSessions
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        public async Task<IEnumerable<CollabSession>> FindByProjectIdAsync(
            int projectId) =>
            await _db.CollabSessions
                .Where(s => s.ProjectId == projectId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<CollabSession>> FindByFileIdAsync(
            int fileId) =>
            await _db.CollabSessions
                .Where(s => s.FileId == fileId)
                .ToListAsync();

        public async Task<IEnumerable<CollabSession>> FindActiveByProjectIdAsync(
            int projectId) =>
            await _db.CollabSessions
                .Where(s => s.ProjectId == projectId
                    && s.Status == "ACTIVE")
                .ToListAsync();

        public async Task<IEnumerable<CollabSession>> FindByOwnerIdAsync(
            int ownerId) =>
            await _db.CollabSessions
                .Where(s => s.OwnerId == ownerId)
                .ToListAsync();

        public async Task<CollabSession?> FindActiveByFileIdAsync(int fileId) =>
            await _db.CollabSessions
                .FirstOrDefaultAsync(s =>
                    s.FileId == fileId && s.Status == "ACTIVE");

        public async Task<CollabSession> CreateAsync(CollabSession session)
        {
            _db.CollabSessions.Add(session);
            await _db.SaveChangesAsync();
            return session;
        }

        public async Task<CollabSession> UpdateAsync(CollabSession session)
        {
            _db.CollabSessions.Update(session);
            await _db.SaveChangesAsync();
            return session;
        }

        public async Task<IEnumerable<Participant>> FindParticipantsBySessionIdAsync(
            Guid sessionId) =>
            await _db.Participants
                .Where(p => p.SessionId == sessionId)
                .ToListAsync();

        public async Task<Participant?> FindParticipantAsync(
            Guid sessionId, int userId) =>
            await _db.Participants
                .FirstOrDefaultAsync(p =>
                    p.SessionId == sessionId && p.UserId == userId);

        public async Task<int> CountParticipantsAsync(Guid sessionId) =>
            await _db.Participants
                .CountAsync(p =>
                    p.SessionId == sessionId && p.IsActive);

        public async Task<Participant> AddParticipantAsync(Participant participant)
        {
            _db.Participants.Add(participant);
            await _db.SaveChangesAsync();
            return participant;
        }

        public async Task<Participant> UpdateParticipantAsync(
            Participant participant)
        {
            _db.Participants.Update(participant);
            await _db.SaveChangesAsync();
            return participant;
        }
    }
}