using Microsoft.Extensions.Options;
using ThriftMedia.Application.Abstractions;

namespace ThriftMedia.Api.Auth;

internal sealed class ConfigSystemAdminAllowlist(IOptions<AuthOptions> options) : ISystemAdminAllowlist
{
    public bool IsSystemAdmin(string provider, string email)
    {
        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.SystemAdminProvider) || string.IsNullOrWhiteSpace(o.SystemAdminEmail))
            return false;

        return string.Equals(o.SystemAdminProvider, provider, StringComparison.Ordinal)
            && string.Equals(o.SystemAdminEmail.Trim(), email?.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
