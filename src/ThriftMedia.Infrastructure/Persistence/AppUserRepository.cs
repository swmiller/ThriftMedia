using Dapper;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Domain.Entities;
using ThriftMedia.Domain.Enums;

namespace ThriftMedia.Infrastructure.Persistence;

internal sealed class AppUserRepository(DbSession session) : IAppUserRepository
{
    private const string SelectColumns = """
        "Id", "Provider", "ProviderSub", "Email", "DisplayName", "Role", "Status", "CreatedAtUtc", "LastSeenAtUtc"
        """;

    public Task<AppUser?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        QuerySingleAsync($"""SELECT {SelectColumns} FROM auth."AppUsers" WHERE "Id" = @id""", new { id }, cancellationToken);

    public Task<AppUser?> GetByProviderAsync(string provider, string providerSub, CancellationToken cancellationToken) =>
        QuerySingleAsync(
            $"""SELECT {SelectColumns} FROM auth."AppUsers" WHERE "Provider" = @provider AND "ProviderSub" = @providerSub""",
            new { provider, providerSub }, cancellationToken);

    public Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        QuerySingleAsync(
            $"""SELECT {SelectColumns} FROM auth."AppUsers" WHERE LOWER("Email") = LOWER(@email)""",
            new { email = email.Trim() }, cancellationToken);

    public Task<AppUser?> GetSystemAdminAsync(CancellationToken cancellationToken) =>
        QuerySingleAsync(
            $"""SELECT {SelectColumns} FROM auth."AppUsers" WHERE "Role" = @role""",
            new { role = (int)AppUserRole.SystemAdmin }, cancellationToken);

    public async Task<int> AddAsync(AppUser user, CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition("""
            INSERT INTO auth."AppUsers"
                ("Provider", "ProviderSub", "Email", "DisplayName", "Role", "Status", "CreatedAtUtc", "LastSeenAtUtc")
            VALUES
                (@Provider, @ProviderSub, @Email, @DisplayName, @Role, @Status, @CreatedAtUtc, @LastSeenAtUtc)
            RETURNING "Id"
            """,
            ToParameters(user), session.Transaction, cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(AppUser user, CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("""
            UPDATE auth."AppUsers"
            SET "Provider" = @Provider, "ProviderSub" = @ProviderSub, "Email" = @Email, "DisplayName" = @DisplayName,
                "Role" = @Role, "Status" = @Status, "LastSeenAtUtc" = @LastSeenAtUtc
            WHERE "Id" = @Id
            """,
            ToParameters(user), session.Transaction, cancellationToken: cancellationToken));
    }

    private async Task<AppUser?> QuerySingleAsync(string sql, object parameters, CancellationToken cancellationToken)
    {
        var connection = await session.GetConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<Row>(
            new CommandDefinition(sql, parameters, session.Transaction, cancellationToken: cancellationToken));
        return row is null ? null : row.ToEntity();
    }

    private static object ToParameters(AppUser user) => new
    {
        user.Id,
        user.Provider,
        user.ProviderSub,
        user.Email,
        user.DisplayName,
        Role = (int)user.Role,
        Status = (int)user.Status,
        CreatedAtUtc = DbTime.ToDb(user.CreatedAtUtc),
        LastSeenAtUtc = DbTime.ToDb(user.LastSeenAtUtc)
    };

    private sealed class Row
    {
        public int Id { get; set; }
        public string? Provider { get; set; }
        public string? ProviderSub { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public int Role { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastSeenAtUtc { get; set; }

        public AppUser ToEntity() => AppUser.Rehydrate(
            Id, Provider, ProviderSub, Email, DisplayName,
            (AppUserRole)Role, (AppUserStatus)Status,
            DbTime.FromDb(CreatedAtUtc), DbTime.FromDb(LastSeenAtUtc));
    }
}
