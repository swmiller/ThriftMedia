using ThriftMedia.Domain.Exceptions;

namespace ThriftMedia.Domain;

/// <summary>Shared argument checks for entities that throw <see cref="DomainValidationException"/>.</summary>
internal static class DomainGuard
{
    public static string Required(string? value, string fieldName, int maxLength)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0) throw new DomainValidationException($"{fieldName} is required");
        if (trimmed.Length > maxLength) throw new DomainValidationException($"{fieldName} max length is {maxLength}");
        return trimmed;
    }

    public static string? Optional(string? value, string fieldName, int maxLength)
    {
        if (value is null) return null;
        var trimmed = value.Trim();
        if (trimmed.Length == 0) return null;
        if (trimmed.Length > maxLength) throw new DomainValidationException($"{fieldName} max length is {maxLength}");
        return trimmed;
    }

    public static int Positive(int value, string fieldName)
    {
        if (value <= 0) throw new DomainValidationException($"{fieldName} must be greater than zero");
        return value;
    }

    /// <summary>Requires a plausible email (one '@', non-empty local and domain parts, no whitespace).</summary>
    public static string Email(string? value, string fieldName)
    {
        var email = Required(value, fieldName, 320);
        var at = email.IndexOf('@');
        if (at <= 0 || at != email.LastIndexOf('@') || at == email.Length - 1
            || email.Any(char.IsWhiteSpace) || !email[(at + 1)..].Contains('.'))
            throw new DomainValidationException($"{fieldName} must be a valid email address");
        return email;
    }

    public static string? HttpUrl(string? value, string fieldName, int maxLength)
    {
        var url = Optional(value, fieldName, maxLength);
        if (url is null) return null;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new DomainValidationException($"{fieldName} must be an http or https URL");
        return url;
    }
}
