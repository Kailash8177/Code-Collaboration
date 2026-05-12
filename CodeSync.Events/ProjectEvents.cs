namespace CodeSync.Events.Projects
{
    // Published by Project-Service when project is created
    public record ProjectCreated(
        int ProjectId,
        int OwnerId,
        string Name,
        string Language,
        string Visibility,
        DateTime CreatedAt
    );

    // Published by Project-Service when project is deleted
    public record ProjectDeleted(
        int ProjectId,
        int OwnerId,
        DateTime DeletedAt
    );

    // Published by Project-Service when project is forked
    public record ProjectForked(
        int OriginalProjectId,
        int ForkedProjectId,
        int ForkedByUserId,
        DateTime ForkedAt
    );

    // Published by Project-Service when member is added
    public record MemberAdded(
        int ProjectId,
        int UserId,
        string Role,
        int AddedByUserId,
        DateTime AddedAt
    );
}