namespace ThriftMedia.Domain.ValueObjects;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other || other.GetType() != GetType()) return false;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var component in GetEqualityComponents())
            {
                hash = hash * 23 + (component?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }
}
