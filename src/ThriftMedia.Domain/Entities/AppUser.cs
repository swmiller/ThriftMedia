using ThriftMedia.Domain.Enums;
using ThriftMedia.Domain.Events;
using ThriftMedia.Domain.Exceptions;

namespace ThriftMedia.Domain.Entities;

/// <summary>
/// An authenticated person. Identity is federated (Google/Microsoft); ThriftMedia stores no credentials.
/// A StoreAdmin is created at approval with no linked identity; the first sign-in links it by verified email.
/// </summary>
public sealed class AppUser : Entity
{
    public int Id { get; private set; }
    public string? Provider { get; private set; }
    public string? ProviderSub { get; private set; }
    public string? Email { get; private set; }
    public string? DisplayName { get; private set; }
    public AppUserRole Role { get; private set; }
    public AppUserStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastSeenAtUtc { get; private set; }

    private AppUser() { }

    public bool HasLinkedIdentity => Provider is not null && ProviderSub is not null;

    /// <summary>True when sign-in and API access are permitted.</summary>
    public bool CanSignIn => Status != AppUserStatus.Suspended;

    /// <summary>Creates the StoreAdmin account when the System Admin approves a store application.</summary>
    public static AppUser CreateStoreAdmin(string email, string? displayName, DateTime nowUtc) => new()
    {
        Email = DomainGuard.Email(email, nameof(Email)),
        DisplayName = DomainGuard.Optional(displayName, nameof(DisplayName), 200),
        Role = AppUserRole.StoreAdmin,
        Status = AppUserStatus.Approved,
        CreatedAtUtc = nowUtc,
        LastSeenAtUtc = nowUtc
    };

    /// <summary>
    /// Creates the System Admin on first sign-in by the allowlisted account. The caller must have
    /// already checked the allowlist and that the provider reported a verified email.
    /// </summary>
    public static AppUser CreateSystemAdmin(string provider, string providerSub, string email, string? displayName, DateTime nowUtc) => new()
    {
        Provider = DomainGuard.Required(provider, nameof(Provider), 200),
        ProviderSub = DomainGuard.Required(providerSub, nameof(ProviderSub), 200),
        Email = DomainGuard.Email(email, nameof(Email)),
        DisplayName = DomainGuard.Optional(displayName, nameof(DisplayName), 200),
        Role = AppUserRole.SystemAdmin,
        Status = AppUserStatus.Active,
        CreatedAtUtc = nowUtc,
        LastSeenAtUtc = nowUtc
    };

    public static AppUser Rehydrate(
        int id, string? provider, string? providerSub, string? email, string? displayName,
        AppUserRole role, AppUserStatus status, DateTime createdAtUtc, DateTime lastSeenAtUtc) => new()
        {
            Id = DomainGuard.Positive(id, nameof(Id)),
            Provider = provider,
            ProviderSub = providerSub,
            Email = email,
            DisplayName = displayName,
            Role = role,
            Status = status,
            CreatedAtUtc = createdAtUtc,
            LastSeenAtUtc = lastSeenAtUtc
        };

    /// <summary>True if <paramref name="email"/> equals this user's email, ignoring case.</summary>
    public bool EmailMatches(string? email) =>
        Email is not null && email is not null && string.Equals(Email, email.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// First sign-in: links the federated identity and activates an Approved account.
    /// The caller must have verified that the provider asserted <c>email_verified</c> and that the
    /// asserted email matches <see cref="Email"/>.
    /// </summary>
    public void LinkIdentity(string provider, string providerSub, string? displayName, DateTime nowUtc)
    {
        if (Status == AppUserStatus.Suspended)
            throw new DomainValidationException("A suspended account cannot sign in");
        if (HasLinkedIdentity)
            throw new DomainValidationException("An identity is already linked to this account");

        Provider = DomainGuard.Required(provider, nameof(Provider), 200);
        ProviderSub = DomainGuard.Required(providerSub, nameof(ProviderSub), 200);
        DisplayName = DomainGuard.Optional(displayName, nameof(DisplayName), 200) ?? DisplayName;
        Status = AppUserStatus.Active;
        LastSeenAtUtc = nowUtc;
    }

    public void RecordSignIn(DateTime nowUtc)
    {
        if (!CanSignIn)
            throw new DomainValidationException("A suspended account cannot sign in");
        if (!HasLinkedIdentity)
            throw new DomainValidationException("Account has no linked identity");

        LastSeenAtUtc = nowUtc;
    }

    /// <summary>Active StoreAdmin to Suspended.</summary>
    public void Suspend(DateTime nowUtc)
    {
        if (Role != AppUserRole.StoreAdmin)
            throw new DomainValidationException("Only a Store Admin can be suspended");
        if (Status != AppUserStatus.Active)
            throw new DomainValidationException($"Only an active account can be suspended (current status: {Status})");

        Status = AppUserStatus.Suspended;
        AddDomainEvent(new AppUserSuspendedEvent(Id, Email!, nowUtc));
    }

    /// <summary>Suspended to Active.</summary>
    public void Reactivate(DateTime nowUtc)
    {
        if (Status != AppUserStatus.Suspended)
            throw new DomainValidationException($"Only a suspended account can be reactivated (current status: {Status})");

        Status = AppUserStatus.Active;
        AddDomainEvent(new AppUserReactivatedEvent(Id, Email!, nowUtc));
    }
}
