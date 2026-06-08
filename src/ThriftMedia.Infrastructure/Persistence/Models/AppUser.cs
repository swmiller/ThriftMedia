using System;
using System.Collections.Generic;

namespace ThriftMedia.Infrastructure.Persistence.Models;

public partial class AppUser
{
    public int Id { get; set; }

    public string Provider { get; set; } = null!;

    public string ProviderSub { get; set; } = null!;

    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    public int Role { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime LastSeenAtUtc { get; set; }

    public virtual Store? Store { get; set; }
}
