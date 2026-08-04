using Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// Use existing external PostgreSQL database (not containerized)
var thriftMediaDb = builder.AddConnectionString("ThriftMediaDb");

var config = builder.Configuration;
config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Run DB migrations before services start
var dbMigrator = builder.AddProject<Projects.ThriftMedia_DbMigrator>("db-migrator")
    .WithReference(thriftMediaDb)
    .WithExplicitStart();

// Register API project and reference PostgreSQL database
var api = builder.AddProject<Projects.ThriftMedia_Api>("api")
    .WithReference(thriftMediaDb);

// Register Admin Portal (Blazor app - store administration)
var admin = builder.AddProject<Projects.ThriftMedia_Admin>("admin")
    .WithReference(api)
    .WithExternalHttpEndpoints();

// Register Consumer Web (Blazor app - public search)
var web = builder.AddProject<Projects.ThriftMedia_Web>("web")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
