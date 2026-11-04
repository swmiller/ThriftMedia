namespace ThriftMedia.Infrastructure.Persistence;

/// <summary>
/// Columns are <c>TIMESTAMP</c> (without time zone) holding UTC. Npgsql refuses to write a
/// <see cref="DateTimeKind.Utc"/> value to such a column, so values are sent as Unspecified
/// and re-tagged as UTC when read.
/// </summary>
internal static class DbTime
{
    public static DateTime ToDb(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Unspecified);

    public static DateTime? ToDb(DateTime? value) => value is null ? null : ToDb(value.Value);

    public static DateTime FromDb(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public static DateTime? FromDb(DateTime? value) => value is null ? null : FromDb(value.Value);
}
