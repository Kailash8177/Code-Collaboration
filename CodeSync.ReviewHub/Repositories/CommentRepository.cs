using CodeSync.ReviewHub.Data;
using CodeSync.ReviewHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeSync.ReviewHub.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ReviewHubDbContext _db;

        public CommentRepository(ReviewHubDbContext db)
        {
            _db = db;
        }

        public async Task<Comment?> FindByCommentIdAsync(int commentId) =>
            await _db.Comments.FindAsync(commentId);

        public async Task<IEnumerable<Comment>> FindByFileIdAsync(int fileId) =>
            await _db.Comments
                .Where(c => c.FileId == fileId)
                .OrderBy(c => c.LineNumber)
                .ToListAsync();

        public async Task<IEnumerable<Comment>> FindByProjectIdAsync(
            int projectId) =>
            await _db.Comments
                .Where(c => c.ProjectId == projectId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Comment>> FindByAuthorIdAsync(
            int authorId) =>
            await _db.Comments
                .Where(c => c.AuthorId == authorId)
                .ToListAsync();

        public async Task<IEnumerable<Comment>> FindByLineNumberAsync(
            int fileId, int lineNumber) =>
            await _db.Comments
                .Where(c => c.FileId == fileId
                    && c.LineNumber == lineNumber)
                .ToListAsync();

        public async Task<IEnumerable<Comment>> FindRepliesAsync(
            int parentCommentId) =>
            await _db.Comments
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Comment>> FindByIsResolvedAsync(
            int projectId, bool isResolved) =>
            await _db.Comments
                .Where(c => c.ProjectId == projectId
                    && c.IsResolved == isResolved)
                .ToListAsync();

        public async Task<int> CountByFileIdAsync(int fileId) =>
            await _db.Comments.CountAsync(c => c.FileId == fileId);

        public async Task<Comment> CreateAsync(Comment comment)
        {
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> UpdateAsync(Comment comment)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            _db.Comments.Update(comment);
            await _db.SaveChangesAsync();
            return comment;
        }

        public async Task DeleteAsync(int commentId)
        {
            var comment = await _db.Comments.FindAsync(commentId);
            if (comment is not null)
            {
                _db.Comments.Remove(comment);
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteByProjectIdAsync(int projectId)
        {
            var comments = await _db.Comments
                .Where(c => c.ProjectId == projectId)
                .ToListAsync();
            _db.Comments.RemoveRange(comments);
            await _db.SaveChangesAsync();
        }
    }
}