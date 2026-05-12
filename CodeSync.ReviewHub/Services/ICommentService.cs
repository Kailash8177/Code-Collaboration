using CodeSync.ReviewHub.DTOs;

namespace CodeSync.ReviewHub.Services
{
    public interface ICommentService
    {
        Task<CommentResponse> AddCommentAsync(
            int authorId, AddCommentRequest request);
        Task<CommentResponse> GetCommentByIdAsync(int commentId);
        Task<IEnumerable<CommentResponse>> GetCommentsByFileAsync(int fileId);
        Task<IEnumerable<CommentResponse>> GetCommentsByProjectAsync(
            int projectId);
        Task<IEnumerable<CommentResponse>> GetCommentsByLineAsync(
            int fileId, int lineNumber);
        Task<IEnumerable<CommentResponse>> GetRepliesAsync(int parentCommentId);
        Task<CommentResponse> UpdateCommentAsync(
            int commentId, int requestingUserId, UpdateCommentRequest request);
        Task DeleteCommentAsync(int commentId, int requestingUserId);
        Task ResolveCommentAsync(int commentId, int requestingUserId);
        Task UnresolveCommentAsync(int commentId, int requestingUserId);
        Task<int> GetCommentCountAsync(int fileId);
    }
}