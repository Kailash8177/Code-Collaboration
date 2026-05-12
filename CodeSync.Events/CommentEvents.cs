namespace CodeSync.Events.Comments
{
    // Published by Comment-Service when comment is added
    public record CommentAdded(
        int CommentId,
        int ProjectId,
        int FileId,
        int AuthorId,
        int LineNumber,
        string Content,
        List<string> MentionedUsernames,
        DateTime CreatedAt
    );

    // Published by Comment-Service when comment is resolved
    public record CommentResolved(
        int CommentId,
        int ProjectId,
        int ResolvedByUserId,
        DateTime ResolvedAt
    );

    public record MentionDetected(
        int CommentId,
        int ProjectId,
        int FileId,
        int AuthorId,
        List<string> MentionedUsernames,
        DateTime CreatedAt
    );
}