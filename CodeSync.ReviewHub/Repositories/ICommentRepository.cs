using CodeSync.ReviewHub.Models;

namespace CodeSync.ReviewHub.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment?> FindByCommentIdAsync(int commentId);
        Task<IEnumerable<Comment>> FindByFileIdAsync(int fileId);
        Task<IEnumerable<Comment>> FindByProjectIdAsync(int projectId);
        Task<IEnumerable<Comment>> FindByAuthorIdAsync(int authorId);
        Task<IEnumerable<Comment>> FindByLineNumberAsync(int fileId, int lineNumber);
        Task<IEnumerable<Comment>> FindRepliesAsync(int parentCommentId);
        Task<IEnumerable<Comment>> FindByIsResolvedAsync(int projectId, bool isResolved);
        Task<int> CountByFileIdAsync(int fileId);
        Task<Comment> CreateAsync(Comment comment);
        Task<Comment> UpdateAsync(Comment comment);
        Task DeleteAsync(int commentId);
        Task DeleteByProjectIdAsync(int projectId);
    }
}