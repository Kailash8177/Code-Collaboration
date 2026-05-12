namespace CodeSync.Events.Collab
{
    // Published by Collab-Service when session starts
    public record SessionStarted(
        Guid SessionId,
        int ProjectId,
        int FileId,
        int OwnerId,
        DateTime StartedAt
    );

    // Published by Collab-Service when session ends
    public record SessionEnded(
        Guid SessionId,
        int ProjectId,
        int OwnerId,
        DateTime EndedAt
    );

    // Published by Collab-Service when user joins
    public record ParticipantJoined(
        Guid SessionId,
        int UserId,
        DateTime JoinedAt
    );

    // Published by Collab-Service when user is kicked
    public record ParticipantKicked(
        Guid SessionId,
        int KickedUserId,
        int KickedByUserId,
        DateTime KickedAt
    );
}