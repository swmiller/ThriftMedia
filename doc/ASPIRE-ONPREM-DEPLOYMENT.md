# Aspire On-Prem Linux Deployment

This project uses .NET Aspire to manage all dependencies, containerization, and orchestration for local and on-premises Linux deployment.

## Container-Based Architecture

All components run as Docker containers, providing:
- **Consistent environments** across development, testing, and production
- **Easy deployment** to any Linux server with Docker installed
- **Resource isolation** and scalability
- **Version control** for all infrastructure components
- **Fast startup** and minimal overhead

## Managed Dependencies
- PostgreSQL (database)
- RabbitMQ (message queue)
- MinIO (object storage)
- All .NET services (API, Admin, Web, Media Processor)

## Local Development
Run the distributed app locally:

```sh
dotnet run --project src/ThriftMedia.AppHost/ThriftMedia.AppHost.csproj
```

## On-Prem Linux Deployment

### Prerequisites
1. Linux server with Docker and Docker Compose installed
2. Sufficient disk space for container images and data volumes
3. Network configuration for service communication

### Deployment Steps
1. Build Aspire containers:
   ```sh
   dotnet publish src/ThriftMedia.AppHost/ThriftMedia.AppHost.csproj -c Release
   ```
2. Export or deploy the generated containers using Docker Compose or your orchestrator of choice.
3. All dependencies (PostgreSQL, RabbitMQ, MinIO, .NET services) run as containers and are managed by Aspire.
4. Data persistence is handled through Docker volumes.

## Container Management

### Services Running as Containers
- **PostgreSQL**: Relational database for application data
- **RabbitMQ**: Message broker for asynchronous processing
- **MinIO**: S3-compatible object storage for media files
- **ThriftMedia.Api**: REST API service
- **ThriftMedia.Admin**: Admin portal (Blazor)
- **ThriftMedia.Web**: Public website (Blazor)
- **ThriftMedia.MediaProcessor**: Background worker service using Akka.NET

### Data Volumes
All stateful services use named Docker volumes for data persistence:
- `postgres-data`: Database storage
- `rabbitmq-data`: Message queue persistence
- `minio-data`: Object storage

## Notes
- All services are containerized for consistent deployment across environments
- Update configuration files as needed for your environment
- Aspire orchestrates the entire container stack without cloud dependencies
- For production, secure your credentials, use secrets management, and configure network access properly
- Consider using Docker Swarm or Kubernetes for high-availability deployments
