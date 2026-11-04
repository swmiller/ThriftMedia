extern alias AdminApp;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using AdminApp::ThriftMedia.Admin.Auth;
using ThriftMedia.Application.Abstractions;
using ThriftMedia.Contracts.Auth;
using ThriftMedia.Tests.Application;

namespace ThriftMedia.Tests.Api;

/// <summary>
/// Hosts the real Api pipeline (authentication, policies, endpoints, handlers) with in-memory repositories,
/// so no database is needed. Tokens are minted by the Admin app's real <see cref="ApiTokenService"/>.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<ThriftMedia.Api.Auth.AuthOptions>
{
    public const string SigningKey = "unit-test-signing-key-0123456789-abcdef";
    public const string AdminEmail = "sysadmin@example.com";

    internal FakeAppUserRepository Users { get; } = new();
    internal FakeStoreApplicationRepository Applications { get; } = new();
    internal FakeStoreRepository Stores { get; } = new();

    static ApiFactory()
    {
        // Program.cs reads the connection string while building, before test overrides apply.
        Environment.SetEnvironmentVariable("ConnectionStrings__ThriftMediaDb", "Host=localhost;Database=none;Username=x;Password=x");
        Environment.SetEnvironmentVariable("Auth__SigningKey", SigningKey);
        Environment.SetEnvironmentVariable("Auth__SystemAdminProvider", "Microsoft");
        Environment.SetEnvironmentVariable("Auth__SystemAdminEmail", AdminEmail);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAppUserRepository>();
            services.RemoveAll<IStoreApplicationRepository>();
            services.RemoveAll<IStoreRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<IAppUserRepository>(Users);
            services.AddSingleton<IStoreApplicationRepository>(Applications);
            services.AddSingleton<IStoreRepository>(Stores);
            services.AddSingleton<IUnitOfWork, FakeUnitOfWork>();
        });
    }

    // ---- token helpers (the Admin app's real minting code) ----

    private static ApiTokenService Minter(TimeProvider? time = null, string key = SigningKey) =>
        new(Options.Create(new TokenOptions { SigningKey = key }), time ?? TimeProvider.System);

    public static string SystemAdminToken(int id = 1) => Minter().CreateUserToken(id, ApiTokenDefaults.RoleSystemAdmin, null);

    public static string StoreAdminToken(int id = 5, int storeId = 10) =>
        Minter().CreateUserToken(id, ApiTokenDefaults.RoleStoreAdmin, storeId);

    public static string SignInToken() => Minter().CreateSignInToken();

    public static string ExpiredToken() =>
        Minter(new FixedTimeProvider(DateTime.UtcNow.AddHours(-1))).CreateUserToken(1, ApiTokenDefaults.RoleSystemAdmin, null);

    public static string TokenSignedWithWrongKey() =>
        Minter(key: "a-completely-different-signing-key-0123456789").CreateUserToken(1, ApiTokenDefaults.RoleSystemAdmin, null);

    /// <summary>A validly signed token with a custom issuer/audience.</summary>
    public static string Custom(string issuer, string audience) =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Claims = new Dictionary<string, object>
            {
                [ApiTokenDefaults.ClaimSubject] = "1",
                [ApiTokenDefaults.ClaimRole] = ApiTokenDefaults.RoleSystemAdmin
            },
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)), SecurityAlgorithms.HmacSha256)
        });

    public HttpClient ClientWith(string? token)
    {
        var client = CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }
}
