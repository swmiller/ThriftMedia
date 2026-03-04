using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database with persistent storage
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var thriftMediaDb = postgres.AddDatabase("ThriftMediaDb");

// Add RabbitMQ for message queue (on-premise, containerized)
var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithDataVolume();

// Add MinIO for object storage (on-premise, S3-compatible, containerized)
var minio = builder.AddContainer("minio", "minio/minio")
    .WithEndpoint(port: 9000, targetPort: 9000, name: "api")
    .WithEndpoint(port: 9001, targetPort: 9001, name: "console")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
    .WithVolume("minio-data", "/data");

// Register API project and reference PostgreSQL database
var api = builder.AddProject<Projects.ThriftMedia_Api>("api")
    .WithReference(thriftMediaDb);

// Register Admin Portal (Blazor app - store administration)
var admin = builder.AddProject<Projects.ThriftMedia_Admin>("admin")
    .WithExternalHttpEndpoints();

// Register Consumer Web (Blazor app - public search)
var web = builder.AddProject<Projects.ThriftMedia_Web>("web")
    .WithExternalHttpEndpoints();

// Register Media Processor Worker Service (uses Akka.NET actors for backend processing)
var mediaProcessor = builder.AddProject<Projects.ThriftMedia_MediaProcessor>("media-processor")
    .WithReference(thriftMediaDb)
    .WithReference(rabbitmq)
    .WithEnvironment("MinIO__Endpoint", "localhost:9000")
    .WithEnvironment("MinIO__AccessKey", "minioadmin")
    .WithEnvironment("MinIO__SecretKey", "minioadmin");

builder.Build().Run();
