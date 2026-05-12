namespace CodeSync.Events.Version
{
    // Published by Version-Service when snapshot is created
    public record SnapshotCreated(
        int SnapshotId,
        int ProjectId,
        int FileId,
        int AuthorId,
        string Message,
        string Branch,
        DateTime CreatedAt
    );

    // Published by Version-Service when file is restored
    public record FileRestored(
        int FileId,
        int ProjectId,
        int SnapshotId,
        int RestoredByUserId,
        DateTime RestoredAt
    );
}