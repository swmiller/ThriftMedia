// This file defines the MongoDB container resource for Aspire local orchestration and on-prem deployment.
using Aspire.Hosting;

public static class MongoResourceExtensions
{
    public static IResourceBuilder<MongoContainerResource> AddMongoDb(this IDistributedApplicationBuilder builder, string name)
    {
        return builder.AddContainer(name, "mongo:7.0")
            .WithVolumeMount("mongo-data", "/data/db")
            .WithEnvironment("MONGO_INITDB_DATABASE", "ThriftMediaDb")
            .WithExposedPort(27017)
            .AsResource<MongoContainerResource>();
    }
}

public class MongoContainerResource : ContainerResource { }
