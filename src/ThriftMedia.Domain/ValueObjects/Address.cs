using ThriftMedia.Domain.Exceptions;

namespace ThriftMedia.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string? ProvinceState { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(string line1, string? line2, string city, string? provinceState, string postalCode, string country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        ProvinceState = provinceState;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string line1, string? line2, string city, string? provinceState, string postalCode, string country)
    {
        line1 = (line1 ?? string.Empty).Trim();
        city = (city ?? string.Empty).Trim();
        postalCode = (postalCode ?? string.Empty).Trim();
        country = (country ?? string.Empty).Trim();

        if (line1.Length == 0) throw new DomainValidationException("Address Line 1 is required");
        if (line1.Length > 150) throw new DomainValidationException("Address Line 1 max length is 150");
        if (city.Length == 0) throw new DomainValidationException("City is required");
        if (city.Length > 100) throw new DomainValidationException("City max length is 100");
        if (postalCode.Length == 0) throw new DomainValidationException("Postal code is required");
        if (postalCode.Length > 20) throw new DomainValidationException("Postal code max length is 20");
        if (country.Length == 0) throw new DomainValidationException("Country is required");
        if (country.Length > 100) throw new DomainValidationException("Country max length is 100");

        var trimmedLine2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        if (trimmedLine2?.Length > 150) throw new DomainValidationException("Address Line 2 max length is 150");

        var trimmedProvince = string.IsNullOrWhiteSpace(provinceState) ? null : provinceState.Trim();
        if (trimmedProvince?.Length > 50) throw new DomainValidationException("Province/State max length is 50");

        return new Address(line1, trimmedLine2, city, trimmedProvince, postalCode, country);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return ProvinceState;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => $"{Line1}, {City}, {ProvinceState} {PostalCode}, {Country}".Trim();
}
