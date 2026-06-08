namespace ThriftMedia.Infrastructure.Persistence.Models
{
    /// <summary>
    /// Defines the mutually exclusive roles an <see cref="AppUser"/> can hold.
    /// A user can only ever be assigned one role.
    /// </summary>
    public enum UserRole
    {
        /// <summary>Platform-level administrator with full system access.</summary>
        SiteAdmin,

        /// <summary>Store-level administrator with access limited to their own store.</summary>
        StoreAdmin
    }
}