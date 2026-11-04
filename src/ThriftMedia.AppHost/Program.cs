using Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL runs in a Docker container.
// Data files are bind-mounted to c:\ThriftMediaDb on the host so they
// survive container restarts and redeployments.
// For PostgreSQL 18+, mount /var/lib/postgresql (not /var/lib/postgresql/data).
var postgres = builder.AddPostgres("postgres")
    .WithBindMount(@"c:\ThriftMediaDb", "/var/lib/postgresql");

var thriftMediaDb = postgres.AddDatabase("ThriftMediaDb");

// Run DB migrations after PostgreSQL is ready and before app services start.
var dbMigrator = builder.AddProject<Projects.ThriftMedia_DbMigrator>("db-migrator")
    .WithReference(thriftMediaDb)
    .WaitFor(postgres);

// ---- Authentication settings (see docs/requirements/features/authentication-authorization.md) ----
// Secrets are never committed. Supply them with user-secrets on this project, e.g.:
//   dotnet user-secrets set "Parameters:google-client-id" "..." --project src/ThriftMedia.AppHost
// Keys: google-client-id, google-client-secret, microsoft-client-id, microsoft-client-secret,
//       system-admin-email (and optionally system-admin-provider, default "Microsoft").
// The token signing key is generated once and persisted to user-secrets automatically.
var signingKey = builder.AddParameter("auth-signing-key",
    new GenerateParameterDefault { MinLength = 48, Special = false }, secret: true, persist: true);

string? Configured(string name) => builder.Configuration[$"Parameters:{name}"] is { Length: > 0 } v ? v : null;

// Only configured providers are offered on the Admin sign-in page.
IResourceBuilder<ParameterResource>? googleId = null, googleSecret = null, msId = null, msSecret = null, adminEmail = null;
if (Configured("google-client-id") is not null && Configured("google-client-secret") is not null)
{
    googleId = builder.AddParameter("google-client-id");
    googleSecret = builder.AddParameter("google-client-secret", secret: true);
}
if (Configured("microsoft-client-id") is not null && Configured("microsoft-client-secret") is not null)
{
    msId = builder.AddParameter("microsoft-client-id");
    msSecret = builder.AddParameter("microsoft-client-secret", secret: true);
}
if (Configured("system-admin-email") is not null)
    adminEmail = builder.AddParameter("system-admin-email");
var adminProvider = Configured("system-admin-provider") ?? "Microsoft";

// Register API project — wait for migrations to finish before starting.
var api = builder.AddProject<Projects.ThriftMedia_Api>("api")
    .WithReference(thriftMediaDb)
    .WithEnvironment("Auth__SigningKey", signingKey)
    .WithEnvironment("Auth__SystemAdminProvider", adminProvider)
    .WaitForCompletion(dbMigrator);
if (adminEmail is not null) api.WithEnvironment("Auth__SystemAdminEmail", adminEmail);

// Register Admin Portal (Blazor app - store administration)
var admin = builder.AddProject<Projects.ThriftMedia_Admin>("admin")
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("Auth__SigningKey", signingKey)
    .WithExternalHttpEndpoints();
if (googleId is not null && googleSecret is not null)
{
    admin.WithEnvironment("Authentication__Google__ClientId", googleId)
         .WithEnvironment("Authentication__Google__ClientSecret", googleSecret);
}
if (msId is not null && msSecret is not null)
{
    admin.WithEnvironment("Authentication__Microsoft__ClientId", msId)
         .WithEnvironment("Authentication__Microsoft__ClientSecret", msSecret);
}

// Register Consumer Web (Blazor app - public search)
var web = builder.AddProject<Projects.ThriftMedia_Web>("web")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
