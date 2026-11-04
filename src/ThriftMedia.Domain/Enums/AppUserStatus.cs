namespace ThriftMedia.Domain.Enums;

/// <summary>Persisted as <c>auth."AppUsers"."Status"</c>. Values must not change.</summary>
public enum AppUserStatus
{
    /// <summary>Account created on approval; waiting for the first federated sign-in to link an identity.</summary>
    Approved = 0,

    /// <summary>Identity linked; the user can sign in.</summary>
    Active = 1,

    /// <summary>Access blocked by the System Admin.</summary>
    Suspended = 2
}
