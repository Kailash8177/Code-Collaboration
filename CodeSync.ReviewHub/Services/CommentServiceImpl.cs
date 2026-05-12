using CodeSync.Events.Comments;
using CodeSync.ReviewHub.DTOs;
using CodeSync.ReviewHub.Models;
using CodeSync.ReviewHub.Repositories;
using MassTransit;
using System.Text.RegularExpressions;

namespace CodeSync.ReviewHub.Services
{
    public class CommentServiceImpl : ICommentService
    {
        private readonly ICommentRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CommentServiceImpl> _logger;

        public CommentServiceImpl(
            ICommentRepository repo,
            IPublishEndpoint publishEndpoint,
            ILogger<CommentServiceImpl> logger)
        {
            _repo            = repo;
            _publishEndpoint = publishEndpoint;
            _logger          = logger;
        }

        // ── Add Comment ───────────────────────────────────────────────────────

        public async Task<CommentResponse> AddCommentAsync(
            int authorId, AddCommentRequest req)
        {
            var comment = new Comment
            {
                ProjectId       = req.ProjectId,
                FileId          = req.FileId,
                AuthorId        = authorId,
                Content         = req.Content,
                LineNumber      = req.LineNumber,
                ColumnNumber    = req.ColumnNumber,
                ParentCommentId = req.ParentCommentId,
                SnapshotId      = req.SnapshotId
            };

            await _repo.CreateAsync(comment);

            // Parse @mentions from comment content
            var mentions = ParseMentions(req.Content);

            // ✅ SAGA — publish MentionDetected event
            // Notification-Service listens → sends @mention alerts
            if (mentions.Any())
            {
                await _publishEndpoint.Publish(new MentionDetected(
                    CommentId:          comment.CommentId,
                    ProjectId:          comment.ProjectId,
                    FileId:             comment.FileId,
                    AuthorId:           authorId,
                    MentionedUsernames: mentions,
                    CreatedAt:          comment.CreatedAt
                ));

                _logger.LogInformation(
                    "MentionDetected published for CommentId={CommentId} Mentions={Count}",
                    comment.CommentId, mentions.Count);
            }

            return MapToResponse(comment);
        }

        // ── Get Comments ──────────────────────────────────────────────────────

        public async Task<CommentResponse> GetCommentByIdAsync(int commentId)
        {
            var comment = await _repo.FindByCommentIdAsync(commentId)
                ?? throw new KeyNotFoundException(
                    $"Comment {commentId} not found.");
            return MapToResponse(comment);
        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsByFileAsync(
            int fileId)
        {
            var comments = await _repo.FindByFileIdAsync(fileId);
            return comments.Select(MapToResponse);
        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsByProjectAsync(
            int projectId)
        {
            var comments = await _repo.FindByProjectIdAsync(projectId);
            return comments.Select(MapToResponse);
        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsByLineAsync(
            int fileId, int lineNumber)
        {
            var comments = await _repo.FindByLineNumberAsync(fileId, lineNumber);
            return comments.Select(MapToResponse);
        }

        public async Task<IEnumerable<CommentResponse>> GetRepliesAsync(
            int parentCommentId)
        {
            var replies = await _repo.FindRepliesAsync(parentCommentId);
            return replies.Select(MapToResponse);
        }

        // ── Update Comment ────────────────────────────────────────────────────

        public async Task<CommentResponse> UpdateCommentAsync(
            int commentId, int requestingUserId, UpdateCommentRequest req)
        {
            var comment = await _repo.FindByCommentIdAsync(commentId)
                ?? throw new KeyNotFoundException(
                    $"Comment {commentId} not found.");

            if (comment.AuthorId != requestingUserId)
                throw new UnauthorizedAccessException(
                    "You can only edit your own comments.");

            comment.Content = req.Content;
            await _repo.UpdateAsync(comment);
            return MapToResponse(comment);
        }

        // ── Delete Comment ────────────────────────────────────────────────────

        public async Task DeleteCommentAsync(int commentId, int requestingUserId)
        {
            var comment = await _repo.FindByCommentIdAsync(commentId)
                ?? throw new KeyNotFoundException(
                    $"Comment {commentId} not found.");

            if (comment.AuthorId != requestingUserId)
                throw new UnauthorizedAccessException(
                    "You can only delete your own comments.");

            await _repo.DeleteAsync(commentId);
        }

        // ── Resolve / Unresolve ───────────────────────────────────────────────

        public async Task ResolveCommentAsync(int commentId, int requestingUserId)
        {
            var comment = await _repo.FindByCommentIdAsync(commentId)
                ?? throw new KeyNotFoundException(
                    $"Comment {commentId} not found.");

            comment.IsResolved = true;
            await _repo.UpdateAsync(comment);
        }

        public async Task UnresolveCommentAsync(
            int commentId, int requestingUserId)
        {
            var comment = await _repo.FindByCommentIdAsync(commentId)
                ?? throw new KeyNotFoundException(
                    $"Comment {commentId} not found.");

            comment.IsResolved = false;
            await _repo.UpdateAsync(comment);
        }

        // ── Count ─────────────────────────────────────────────────────────────

        public async Task<int> GetCommentCountAsync(int fileId)
            => await _repo.CountByFileIdAsync(fileId);

        // ── Helpers ───────────────────────────────────────────────────────────

        private static List<string> ParseMentions(string content)
        {
            var matches = Regex.Matches(content, @"@(\w+)");
            return matches
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();
        }

        private static CommentResponse MapToResponse(Comment c) => new()
        {
            CommentId       = c.CommentId,
            ProjectId       = c.ProjectId,
            FileId          = c.FileId,
            AuthorId        = c.AuthorId,
            Content         = c.Content,
            LineNumber      = c.LineNumber,
            ColumnNumber    = c.ColumnNumber,
            ParentCommentId = c.ParentCommentId,
            IsResolved      = c.IsResolved,
            SnapshotId      = c.SnapshotId,
            CreatedAt       = c.CreatedAt,
            UpdatedAt       = c.UpdatedAt
        };
    }
}