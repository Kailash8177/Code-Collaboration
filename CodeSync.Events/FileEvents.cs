namespace CodeSync.Events.Files
{
    // Published by File-Service when file is created
    public record FileCreated(
        int FileId,
        int ProjectId,
        string Name,
        string Path,
        int CreatedById,
        DateTime CreatedAt
    );

    // Published by File-Service when file content is updated
    public record FileUpdated(
        int FileId,
        int ProjectId,
        string Name,
        int UpdatedByUserId,
        DateTime UpdatedAt
    );

    // Published by File-Service when file is deleted
    public record FileDeleted(
        int FileId,
        int ProjectId,
        DateTime DeletedAt
    );
}