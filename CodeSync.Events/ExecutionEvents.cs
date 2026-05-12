namespace CodeSync.Events.Execution
{
    public record CodeExecuted(
        Guid JobId,
        int UserId,
        int ProjectId,
        string Language,
        string Status,
        int ExecutionTimeMs,
        DateTime CompletedAt
    );

    public record ExecutionFailed(
        Guid JobId,
        int UserId,
        int ProjectId,
        string Language,
        string ErrorMessage,
        DateTime FailedAt
    );
}