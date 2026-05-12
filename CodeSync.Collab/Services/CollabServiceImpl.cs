using CodeSync.Collab.DTOs;
using CodeSync.Collab.Models;
using CodeSync.Collab.Repositories;
using CodeSync.Events.Collab;
using MassTransit;

namespace CodeSync.Collab.Services
{
    public class CollabServiceImpl : ICollabService
    {
        private readonly ICollabRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CollabServiceImpl> _logger;

        private static readonly string[] Colors = new[]
        {
            "#FF5733", "#33FF57", "#3357FF", "#FF33F5",
            "#F5FF33", "#33FFF5", "#FF8C33", "#8C33FF"
        };

        public CollabServiceImpl(
            ICollabRepository repo,
            IPublishEndpoint publishEndpoint,
            ILogger<CollabServiceImpl> logger)
        {
            _repo            = repo;
            _publishEndpoint = publishEndpoint;
            _logger          = logger;
        }

        // ── Create Session ────────────────────────────────────────────────────

        public async Task<SessionResponse> CreateSessionAsync(
            int ownerId, CreateSessionRequest req)
        {
            var session = new CollabSession
            {
                ProjectId           = req.ProjectId,
                FileId              = req.FileId,
                OwnerId             = ownerId,
                Language            = req.Language,
                MaxParticipants     = req.MaxParticipants,
                IsPasswordProtected = req.IsPasswordProtected,
                SessionPassword     = req.IsPasswordProtected
                    ? req.SessionPassword : null,
                Status = "ACTIVE"
            };

            await _repo.CreateAsync(session);

            // Owner is HOST automatically
            await _repo.AddParticipantAsync(new Participant
            {
                SessionId = session.SessionId,
                UserId    = ownerId,
                Role      = "HOST",
                Color     = Colors[0],
                IsActive  = true
            });

            // ✅ SAGA — publish SessionStarted event
            await _publishEndpoint.Publish(new SessionStarted(
                SessionId: session.SessionId,
                ProjectId: session.ProjectId,
                FileId:    session.FileId,
                OwnerId:   ownerId,
                StartedAt: session.CreatedAt
            ));

            _logger.LogInformation(
                "SessionStarted published for SessionId={SessionId}",
                session.SessionId);

            return MapToResponse(session, 1);
        }

        // ── Get Session ───────────────────────────────────────────────────────

        public async Task<SessionResponse> GetSessionByIdAsync(Guid sessionId)
        {
            var session = await _repo.FindBySessionIdAsync(sessionId)
                ?? throw new KeyNotFoundException(
                    $"Session {sessionId} not found.");
            var count = await _repo.CountParticipantsAsync(sessionId);
            return MapToResponse(session, count);
        }

        // ── Get Sessions ──────────────────────────────────────────────────────

        public async Task<IEnumerable<SessionResponse>> GetSessionsByProjectAsync(
            int projectId)
        {
            var sessions = await _repo.FindByProjectIdAsync(projectId);
            var result   = new List<SessionResponse>();
            foreach (var s in sessions)
            {
                var count = await _repo.CountParticipantsAsync(s.SessionId);
                result.Add(MapToResponse(s, count));
            }
            return result;
        }

        public async Task<IEnumerable<SessionResponse>> GetActiveSessionsByProjectAsync(
            int projectId)
        {
            var sessions = await _repo.FindActiveByProjectIdAsync(projectId);
            var result   = new List<SessionResponse>();
            foreach (var s in sessions)
            {
                var count = await _repo.CountParticipantsAsync(s.SessionId);
                result.Add(MapToResponse(s, count));
            }
            return result;
        }

        public async Task<SessionResponse?> GetActiveSessionByFileAsync(int fileId)
        {
            var session = await _repo.FindActiveByFileIdAsync(fileId);
            if (session is null) return null;
            var count = await _repo.CountParticipantsAsync(session.SessionId);
            return MapToResponse(session, count);
        }

        // ── Join Session ──────────────────────────────────────────────────────

        public async Task<ParticipantResponse> JoinSessionAsync(
            int userId, JoinSessionRequest req)
        {
            var session = await _repo.FindBySessionIdAsync(req.SessionId)
                ?? throw new KeyNotFoundException(
                    $"Session {req.SessionId} not found.");

            if (session.Status == "ENDED")
                throw new InvalidOperationException("Session has ended.");

            if (session.IsPasswordProtected)
            {
                if (string.IsNullOrEmpty(req.Password) ||
                    req.Password != session.SessionPassword)
                    throw new UnauthorizedAccessException(
                        "Incorrect session password.");
            }

            var count = await _repo.CountParticipantsAsync(req.SessionId);
            if (count >= session.MaxParticipants)
                throw new InvalidOperationException("Session is full.");

            // Check if already joined
            var existing = await _repo
                .FindParticipantAsync(req.SessionId, userId);
            if (existing is not null)
            {
                existing.IsActive = true;
                existing.LeftAt   = null;
                await _repo.UpdateParticipantAsync(existing);
                return MapToParticipant(existing);
            }

            var color       = Colors[count % Colors.Length];
            var participant = new Participant
            {
                SessionId = req.SessionId,
                UserId    = userId,
                Role      = "EDITOR",
                Color     = color,
                IsActive  = true
            };

            await _repo.AddParticipantAsync(participant);
            await UpdateLastActivityAsync(req.SessionId);

            // ✅ SAGA — publish ParticipantJoined event
            await _publishEndpoint.Publish(new ParticipantJoined(
                SessionId: req.SessionId,
                UserId:    userId,
                JoinedAt:  DateTime.UtcNow
            ));

            _logger.LogInformation(
                "ParticipantJoined published. SessionId={SessionId} UserId={UserId}",
                req.SessionId, userId);

            return MapToParticipant(participant);
        }

        // ── Leave Session ─────────────────────────────────────────────────────

        public async Task LeaveSessionAsync(Guid sessionId, int userId)
        {
            var participant = await _repo
                .FindParticipantAsync(sessionId, userId)
                ?? throw new KeyNotFoundException("Participant not found.");

            participant.IsActive = false;
            participant.LeftAt   = DateTime.UtcNow;
            await _repo.UpdateParticipantAsync(participant);
        }

        // ── End Session ───────────────────────────────────────────────────────

        public async Task EndSessionAsync(Guid sessionId, int requestingUserId)
        {
            var session = await _repo.FindBySessionIdAsync(sessionId)
                ?? throw new KeyNotFoundException(
                    $"Session {sessionId} not found.");

            if (session.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException(
                    "Only the session owner can end the session.");

            session.Status  = "ENDED";
            session.EndedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(session);

            // Mark all participants inactive
            var participants = await _repo
                .FindParticipantsBySessionIdAsync(sessionId);
            foreach (var p in participants.Where(p => p.IsActive))
            {
                p.IsActive = false;
                p.LeftAt   = DateTime.UtcNow;
                await _repo.UpdateParticipantAsync(p);
            }

            // ✅ SAGA — publish SessionEnded event
            await _publishEndpoint.Publish(new SessionEnded(
                SessionId: sessionId,
                ProjectId: session.ProjectId,
                OwnerId:   requestingUserId,
                EndedAt:   DateTime.UtcNow
            ));

            _logger.LogInformation(
                "SessionEnded published for SessionId={SessionId}", sessionId);
        }

        // ── Kick Participant ──────────────────────────────────────────────────

        public async Task KickParticipantAsync(
            Guid sessionId, int requestingUserId, int targetUserId)
        {
            var session = await _repo.FindBySessionIdAsync(sessionId)
                ?? throw new KeyNotFoundException(
                    $"Session {sessionId} not found.");

            if (session.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException(
                    "Only the session owner can kick participants.");

            var participant = await _repo
                .FindParticipantAsync(sessionId, targetUserId)
                ?? throw new KeyNotFoundException("Participant not found.");

            participant.IsActive = false;
            participant.LeftAt   = DateTime.UtcNow;
            await _repo.UpdateParticipantAsync(participant);

            // ✅ SAGA — publish ParticipantKicked event
            await _publishEndpoint.Publish(new ParticipantKicked(
                SessionId:      sessionId,
                KickedUserId:   targetUserId,
                KickedByUserId: requestingUserId,
                KickedAt:       DateTime.UtcNow
            ));

            _logger.LogInformation(
                "ParticipantKicked published. SessionId={SessionId} UserId={UserId}",
                sessionId, targetUserId);
        }

        // ── Get Participants ──────────────────────────────────────────────────

        public async Task<IEnumerable<ParticipantResponse>> GetParticipantsAsync(
            Guid sessionId)
        {
            var participants = await _repo
                .FindParticipantsBySessionIdAsync(sessionId);
            return participants.Select(MapToParticipant);
        }

        // ── Update Cursor ─────────────────────────────────────────────────────

        public async Task UpdateCursorAsync(
            Guid sessionId, int userId, int line, int col)
        {
            var participant = await _repo
                .FindParticipantAsync(sessionId, userId);
            if (participant is null) return;

            participant.CursorLine = line;
            participant.CursorCol  = col;
            await _repo.UpdateParticipantAsync(participant);
            await UpdateLastActivityAsync(sessionId);
        }

        // ── Update Last Activity ──────────────────────────────────────────────

        public async Task UpdateLastActivityAsync(Guid sessionId)
        {
            var session = await _repo.FindBySessionIdAsync(sessionId);
            if (session is null) return;
            session.LastActivityAt = DateTime.UtcNow;
            await _repo.UpdateAsync(session);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static SessionResponse MapToResponse(
            CollabSession s, int count) => new()
        {
            SessionId           = s.SessionId,
            ProjectId           = s.ProjectId,
            FileId              = s.FileId,
            OwnerId             = s.OwnerId,
            Status              = s.Status,
            Language            = s.Language,
            MaxParticipants     = s.MaxParticipants,
            IsPasswordProtected = s.IsPasswordProtected,
            CreatedAt           = s.CreatedAt,
            EndedAt             = s.EndedAt,
            ParticipantCount    = count
        };

        private static ParticipantResponse MapToParticipant(
            Participant p) => new()
        {
            ParticipantId = p.ParticipantId,
            SessionId     = p.SessionId,
            UserId        = p.UserId,
            Role          = p.Role,
            CursorLine    = p.CursorLine,
            CursorCol     = p.CursorCol,
            Color         = p.Color,
            IsActive      = p.IsActive,
            JoinedAt      = p.JoinedAt
        };
    }
}