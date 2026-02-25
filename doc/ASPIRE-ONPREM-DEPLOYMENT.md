# Aspire On-Prem Linux Deployment

This project uses .NET Aspire to manage all dependencies, containerization, and orchestration for local and on-premises Linux deployment.

## Managed Dependencies
- PostgreSQL (SQL database)
- MongoDB (NoSQL database)
- Azure Service Bus (emulated)
- Azure Blob Storage (emulated)
- All .NET services (API, Admin, Web, Media Processor)

## Local Development
Run the distributed app locally:

```sh
dotnet run --project src/ThriftMedia.AppHost/ThriftMedia.AppHost.csproj
```

## On-Prem Linux Deployment
1. Ensure Docker is installed on your Linux host.
2. Build Aspire containers:
   ```sh
   dotnet publish src/ThriftMedia.AppHost/ThriftMedia.AppHost.csproj -c Release
   ```
3. Export or deploy the generated containers using Docker Compose or your orchestrator of choice.
4. All dependencies (databases, services) are managed and started by Aspire.

## Notes
- Update configuration files as needed for your environment.
- Aspire does not require Azure; it works for on-prem and hybrid scenarios.
- For production, secure your database credentials and network access.
