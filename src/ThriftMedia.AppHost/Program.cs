using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database with persistent storage
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var thriftMediaDb = postgres.AddDatabase("ThriftMediaDb");

// Register API project and reference the database
builder.AddProject<Projects.ThriftMedia_Api>("thriftmediaapi")
    .WithReference(thriftMediaDb);

// Register Web project
builder.AddProject<Projects.ThriftMedia_Web>("thriftmediaweb");

builder.Build().Run();
