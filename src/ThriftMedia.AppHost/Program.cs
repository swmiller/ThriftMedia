using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database with persistent storage
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var thriftMediaDb = postgres.AddDatabase("ThriftMediaDb");

// Add Azure Service Bus for media processing queue
var serviceBus = builder.AddAzureServiceBus("servicebus")
    .RunAsEmulator();
var mediaProcessingQueue = serviceBus.AddServiceBusQueue("media-processing");

// Add Azure Blob Storage for media images
var blobStorage = builder.AddAzureStorage("storage")
    .RunAsEmulator();
var mediaImages = blobStorage.AddBlobs("media-images");

// Register API project and reference the database
var api = builder.AddProject<Projects.ThriftMedia_Api>("api")
    .WithReference(thriftMediaDb);

// Register Admin Portal (Blazor app - store administration)
var admin = builder.AddProject<Projects.ThriftMedia_Admin>("admin")
    .WithExternalHttpEndpoints();

// Register Consumer Web (Angular app - public search)
var web = builder.AddNpmApp("web", "../ThriftMedia.Web", "start")
    .WithHttpEndpoint(port: 5002, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

// Register Media Processor Worker Service
var mediaProcessor = builder.AddProject<Projects.ThriftMedia_MediaProcessor>("media-processor")
    .WithReference(thriftMediaDb)
    .WithReference(mediaProcessingQueue)
    .WithReference(mediaImages);

builder.Build().Run();
