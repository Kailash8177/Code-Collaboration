namespace CodeSync.Events.Users
{
    // Published by Auth-Service when user registers
    public record UserRegistered(
        int UserId,
        string Username,
        string Email,
        string FullName,
        DateTime RegisteredAt
    );

    // Published by Auth-Service when user is deactivated
    public record UserDeactivated(
        int UserId,
        DateTime DeactivatedAt
    );
}