namespace ThriftMedia.Application.Abstractions;

public static class CurrentUserExtensions
{
    /// <summary>Returns the System Admin's AppUserId or throws <see cref="Common.ForbiddenException"/>.</summary>
    public static int RequireSystemAdmin(this ICurrentUser currentUser)
    {
        if (!currentUser.IsSystemAdmin || currentUser.AppUserId is not int id)
            throw new Common.ForbiddenException("System Admin access is required.");

        return id;
    }
}
