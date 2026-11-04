namespace ThriftMedia.Application.Abstractions;

/// <summary>
/// Identifies the single System Admin. Backed by configuration (never committed): the allowed
/// provider and email address. Only that provider account can ever obtain the SystemAdmin role.
/// </summary>
public interface ISystemAdminAllowlist
{
    bool IsSystemAdmin(string provider, string email);
}
