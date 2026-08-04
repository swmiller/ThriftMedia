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

// Register API project — wait for migrations to finish before starting.
var api = builder.AddProject<Projects.ThriftMedia_Api>("api")
    .WithReference(thriftMediaDb)
    .WaitForCompletion(dbMigrator);

// Register Admin Portal (Blazor app - store administration)
var admin = builder.AddProject<Projects.ThriftMedia_Admin>("admin")
    .WithReference(api)
    .WithExternalHttpEndpoints();

// Register Consumer Web (Blazor app - public search)
var web = builder.AddProject<Projects.ThriftMedia_Web>("web")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
