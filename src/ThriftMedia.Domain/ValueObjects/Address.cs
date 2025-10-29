using ThriftMedia.Domain.Exceptions;

namespace ThriftMedia.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    private Address(string street, string city, string state, string zipCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    public static Address Create(string street, string city, string state, string zipCode, string country = "USA")
    {
        street = (street ?? string.Empty).Trim();
        city = (city ?? string.Empty).Trim();
        state = (state ?? string.Empty).Trim().ToUpperInvariant();
        zipCode = (zipCode ?? string.Empty).Trim();
        country = (country ?? string.Empty).Trim();

        if (street.Length == 0) throw new DomainValidationException("Street is required");
        if (city.Length == 0) throw new DomainValidationException("City is required");
        if (state.Length != 2) throw new DomainValidationException("State must be 2 characters");
        if (zipCode.Length is < 5 or > 10) throw new DomainValidationException("ZipCode length invalid");
        if (country.Length == 0) throw new DomainValidationException("Country is required");

        return new Address(street, city, state, zipCode, country);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
