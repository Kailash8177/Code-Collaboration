namespace CodeSync.Events.Notifications
{
    // Published when any notification needs to be sent
    public record SendNotification(
        int RecipientId,
        int ActorId,
        string Type,        // SESSION_INVITE | COMMENT | MENTION | SNAPSHOT | FORK
        string Title,
        string Message,
        string RelatedId,
        string RelatedType,
        bool SendEmail,
        DateTime CreatedAt
    );

    // Published for bulk notifications
    public record SendBulkNotification(
        List<int> RecipientIds,
        string Type,
        string Title,
        string Message,
        DateTime CreatedAt
    );
}