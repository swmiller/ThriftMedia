# NoSQL Migration Plan: PostgreSQL to MongoDB

## Executive Summary

This document outlines the migration from PostgreSQL to MongoDB for cost reduction while maintaining functionality. MongoDB will run as a container alongside the application on the same Linux server.

---

## Why MongoDB?

### Advantages for ThriftMedia

✅ **Cost Effective**: Free, open-source, no licensing costs  
✅ **Co-location**: Runs efficiently in Docker container on same server  
✅ **Simple Deployment**: Single Docker container, minimal configuration  
✅ **.NET Support**: Excellent with `MongoDB.Driver` NuGet package  
✅ **Flexible Schema**: Easy to evolve as requirements change  
✅ **Good for Media Inventory**: Document model fits media items naturally  
✅ **Handles Both**: Simple key-value lookups AND complex queries  

### Comparison to Azure Table Storage

| Feature | Azure Table Storage | MongoDB (Self-Hosted) |
|---------|---------------------|------------------------|
| Cost | Pay per usage | Free (self-hosted) |
| Query Flexibility | Limited (partition + row key) | Rich query language |
| Indexes | Limited | Multiple indexes supported |
| Transactions | Limited | Multi-document ACID transactions |
| Schema | Schema-less | Flexible schema with validation |
| Aggregation | No | Powerful aggregation framework |
| .NET Support | Good | Excellent |
| Linux Compatible | N/A (cloud only) | ✅ Native Linux support |

---

## Architecture Changes

### Current Architecture (PostgreSQL)

```
┌─────────────────────────────────────┐
│   ThriftMedia.Infrastructure        │
│                                     │
│   ┌─────────────────────────────┐  │
│   │  EF Core + Npgsql          │  │
│   │  ThriftMediaDbContext       │  │
│   └─────────────┬───────────────┘  │
└─────────────────┼───────────────────┘
                  │
           ┌──────▼──────┐
           │  PostgreSQL │
           │  Container  │
           └─────────────┘
```

### New Architecture (MongoDB)

```
┌─────────────────────────────────────┐
│   ThriftMedia.Infrastructure        │
│                                     │
│   ┌─────────────────────────────┐  │
│   │  MongoDB.Driver            │  │
│   │  MongoDbContext (custom)    │  │
│   └─────────────┬───────────────┘  │
└─────────────────┼───────────────────┘
                  │
           ┌──────▼──────┐
           │   MongoDB   │
           │  Container  │
           └─────────────┘
```

---

## Data Model Changes

### PostgreSQL (Relational)

```csharp
// Stores Table
Store {
    Id: Guid (PK)
    Name: string
    Address: Address (value object)
    OwnerId: Guid
    CreatedAt: DateTime
}

// Media Table
Media {
    Id: Guid (PK)
    StoreId: Guid (FK)
    Title: string
    MediaType: string
    Status: string
    ImageUrl: string
    CreatedAt: DateTime
}
```

### MongoDB (Document)

```csharp
// Stores Collection
{
    "_id": ObjectId("...") or Guid,
    "name": "Thrift Store Downtown",
    "address": {
        "street": "123 Main St",
        "city": "Seattle",
        "state": "WA",
        "zipCode": "98101",
        "country": "USA"
    },
    "ownerId": "user-guid",
    "auditMetadata": {
        "createdAt": ISODate("2026-02-25T..."),
        "createdBy": "user-guid",
        "updatedAt": ISODate("2026-02-25T..."),
        "updatedBy": "user-guid"
    }
}

// Media Collection
{
    "_id": ObjectId("...") or Guid,
    "storeId": "store-guid",
    "title": "The Great Gatsby",
    "mediaType": "Book",
    "status": "Available",
    "imageUrl": "https://...",
    "description": "Classic novel...",
    "auditMetadata": {
        "createdAt": ISODate("2026-02-25T..."),
        "createdBy": "user-guid"
    }
}
```

**Key Differences:**
- No foreign keys (document references instead)
- Embedded documents (Address inside Store)
- Flexible schema (can add fields without migrations)
- Native GUID or ObjectId support

---

## Implementation Plan

### Phase 1: Setup and Infrastructure (Week 1)

#### 1.1 Add MongoDB Dependencies

**Update `ThriftMedia.Infrastructure.csproj`:**

```xml
<ItemGroup>
    <!-- Remove PostgreSQL -->
    <PackageReference Remove="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
    
    <!-- Add MongoDB -->
    <PackageReference Include="MongoDB.Driver" Version="2.28.0" />
    <PackageReference Include="MongoDB.Bson" Version="2.28.0" />
</ItemGroup>
```

#### 1.2 Create MongoDB Context

**Create `Infrastructure/Persistence/MongoDbContext.cs`:**

```csharp
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace ThriftMedia.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<Store> Stores => 
        _database.GetCollection<Store>("stores");
    
    public IMongoCollection<Media> Media => 
        _database.GetCollection<Media>("media");
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
```

#### 1.3 Update Domain Entities for MongoDB

**Add to Domain entities:**

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Store
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; private set; }
    
    [BsonElement("name")]
    public string Name { get; private set; }
    
    [BsonElement("address")]
    public Address Address { get; private set; }
    
    [BsonElement("ownerId")]
    [BsonRepresentation(BsonType.String)]
    public Guid OwnerId { get; private set; }
    
    [BsonElement("auditMetadata")]
    public AuditMetadata AuditMetadata { get; private set; }
    
    // Existing methods...
}
```

#### 1.4 Update AppHost for MongoDB Container

**Update `ThriftMedia.AppHost/Program.cs`:**

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Remove PostgreSQL
// var postgres = builder.AddPostgres("postgres")
//                       .WithPgAdmin()
//                       .AddDatabase("thriftmediadb");

// Add MongoDB
var mongodb = builder.AddMongoDB("mongodb")
                     .WithMongoExpress()  // Optional: Web UI for MongoDB
                     .AddDatabase("thriftmediadb");

var servicebus = builder.AddAzureServiceBus("servicebus");
var storage = builder.AddAzureStorage("storage").RunAsEmulator();

var api = builder.AddProject<Projects.ThriftMedia_Api>("thriftmediaapi")
                 .WithReference(mongodb)  // Changed from postgres
                 .WithReference(storage);

// ... rest of configuration
```

#### 1.5 Update Configuration

**Update `appsettings.json`:**

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "thriftmediadb"
  }
}
```

**For .NET Aspire (handled automatically):**
```csharp
// In Program.cs
builder.AddMongoDBClient("mongodb");
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
```

### Phase 2: Repository Implementation (Week 2)

#### 2.1 Create MongoDB Repositories

**Create `Infrastructure/Repositories/MongoStoreRepository.cs`:**

```csharp
using MongoDB.Driver;
using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Infrastructure.Repositories;

public class MongoStoreRepository : IStoreRepository
{
    private readonly IMongoCollection<Store> _stores;

    public MongoStoreRepository(MongoDbContext context)
    {
        _stores = context.Stores;
    }

    public async Task<Store?> GetByIdAsync(Guid id)
    {
        return await _stores.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Store>> GetAllAsync()
    {
        return await _stores.Find(_ => true).ToListAsync();
    }

    public async Task<Store> AddAsync(Store store)
    {
        await _stores.InsertOneAsync(store);
        return store;
    }

    public async Task UpdateAsync(Store store)
    {
        await _stores.ReplaceOneAsync(s => s.Id == store.Id, store);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _stores.DeleteOneAsync(s => s.Id == id);
    }

    // Location-based query (requires geospatial index)
    public async Task<IEnumerable<Store>> FindNearLocationAsync(
        double latitude, double longitude, double radiusMiles)
    {
        var point = GeoJson.Point(GeoJson.Position(longitude, latitude));
        var locationFilter = Builders<Store>.Filter.Near(
            s => s.Address.Location, point, radiusMiles * 1609.34); // miles to meters

        return await _stores.Find(locationFilter).ToListAsync();
    }
}
```

**Create `Infrastructure/Repositories/MongoMediaRepository.cs`:**

```csharp
using MongoDB.Driver;
using ThriftMedia.Domain.Entities;

namespace ThriftMedia.Infrastructure.Repositories;

public class MongoMediaRepository : IMediaRepository
{
    private readonly IMongoCollection<Media> _media;

    public MongoMediaRepository(MongoDbContext context)
    {
        _media = context.Media;
    }

    public async Task<Media?> GetByIdAsync(Guid id)
    {
        return await _media.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Media>> GetByStoreIdAsync(Guid storeId)
    {
        return await _media.Find(m => m.StoreId == storeId).ToListAsync();
    }

    public async Task<IEnumerable<Media>> SearchAsync(
        string? searchTerm, 
        string? mediaType, 
        string? status,
        int skip = 0,
        int take = 50)
    {
        var filterBuilder = Builders<Media>.Filter;
        var filters = new List<FilterDefinition<Media>>();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            filters.Add(filterBuilder.Regex(m => m.Title, 
                new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")));
        }

        if (!string.IsNullOrEmpty(mediaType))
        {
            filters.Add(filterBuilder.Eq(m => m.MediaType, mediaType));
        }

        if (!string.IsNullOrEmpty(status))
        {
            filters.Add(filterBuilder.Eq(m => m.Status, status));
        }

        var filter = filters.Any() 
            ? filterBuilder.And(filters) 
            : filterBuilder.Empty;

        return await _media.Find(filter)
                           .Skip(skip)
                           .Limit(take)
                           .ToListAsync();
    }

    public async Task<Media> AddAsync(Media media)
    {
        await _media.InsertOneAsync(media);
        return media;
    }

    public async Task UpdateAsync(Media media)
    {
        await _media.ReplaceOneAsync(m => m.Id == media.Id, media);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _media.DeleteOneAsync(m => m.Id == id);
    }
}
```

#### 2.2 Create Indexes

**Create `Infrastructure/Persistence/MongoDbIndexes.cs`:**

```csharp
using MongoDB.Driver;

namespace ThriftMedia.Infrastructure.Persistence;

public static class MongoDbIndexes
{
    public static async Task CreateIndexesAsync(MongoDbContext context)
    {
        // Store indexes
        var storeIndexes = new[]
        {
            new CreateIndexModel<Store>(
                Builders<Store>.IndexKeys.Ascending(s => s.OwnerId),
                new CreateIndexOptions { Name = "idx_store_ownerId" }
            ),
            new CreateIndexModel<Store>(
                Builders<Store>.IndexKeys.Geo2DSphere("address.location"),
                new CreateIndexOptions { Name = "idx_store_location" }
            )
        };
        await context.Stores.Indexes.CreateManyAsync(storeIndexes);

        // Media indexes
        var mediaIndexes = new[]
        {
            new CreateIndexModel<Media>(
                Builders<Media>.IndexKeys.Ascending(m => m.StoreId),
                new CreateIndexOptions { Name = "idx_media_storeId" }
            ),
            new CreateIndexModel<Media>(
                Builders<Media>.IndexKeys.Ascending(m => m.Status),
                new CreateIndexOptions { Name = "idx_media_status" }
            ),
            new CreateIndexModel<Media>(
                Builders<Media>.IndexKeys.Text(m => m.Title),
                new CreateIndexOptions { Name = "idx_media_title_text" }
            )
        };
        await context.Media.Indexes.CreateManyAsync(mediaIndexes);
    }
}
```

**Call from `Program.cs`:**

```csharp
// After app is built, before app.Run()
using (var scope = app.Services.CreateScope())
{
    var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await MongoDbIndexes.CreateIndexesAsync(mongoContext);
}
```

### Phase 3: Update Application Layer (Week 3)

#### 3.1 Update Command Handlers

**Example: `CreateStoreCommandHandler.cs`:**

```csharp
public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, Guid>
{
    private readonly IStoreRepository _storeRepository;

    public CreateStoreCommandHandler(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<Guid> Handle(CreateStoreCommand request, CancellationToken ct)
    {
        // Validation already done by FluentValidation
        
        var store = Store.Create(
            request.Name,
            request.Address,
            request.OwnerId
        );

        await _storeRepository.AddAsync(store);
        
        // No need for SaveChanges() - MongoDB saves immediately
        
        return store.Id;
    }
}
```

**Key Difference from EF Core:**
- No `DbContext.SaveChanges()` - MongoDB operations are immediately persisted
- No transaction management needed for single operations
- For multi-document transactions, use MongoDB sessions

#### 3.2 Multi-Document Transactions (if needed)

**Create `Infrastructure/UnitOfWork/MongoUnitOfWork.cs`:**

```csharp
public interface IMongoUnitOfWork : IDisposable
{
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task AbortTransactionAsync();
}

public class MongoUnitOfWork : IMongoUnitOfWork
{
    private readonly IMongoClient _client;
    private IClientSessionHandle? _session;

    public MongoUnitOfWork(IMongoClient client)
    {
        _client = client;
    }

    public async Task BeginTransactionAsync()
    {
        _session = await _client.StartSessionAsync();
        _session.StartTransaction();
    }

    public async Task CommitTransactionAsync()
    {
        if (_session != null)
        {
            await _session.CommitTransactionAsync();
        }
    }

    public async Task AbortTransactionAsync()
    {
        if (_session != null)
        {
            await _session.AbortTransactionAsync();
        }
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
```

### Phase 4: Data Migration (Week 4)

#### 4.1 Create Migration Tool

**Create `tools/DataMigration/PostgresToMongo.cs`:**

```csharp
using Npgsql;
using MongoDB.Driver;

public class PostgresToMongoMigration
{
    private readonly string _postgresConnection;
    private readonly IMongoDatabase _mongoDatabase;

    public PostgresToMongoMigration(string postgresConnection, IMongoDatabase mongoDatabase)
    {
        _postgresConnection = postgresConnection;
        _mongoDatabase = mongoDatabase;
    }

    public async Task MigrateAsync()
    {
        Console.WriteLine("Starting migration from PostgreSQL to MongoDB...");

        await MigrateStoresAsync();
        await MigrateMediaAsync();

        Console.WriteLine("Migration complete!");
    }

    private async Task MigrateStoresAsync()
    {
        var storesCollection = _mongoDatabase.GetCollection<Store>("stores");
        
        using var conn = new NpgsqlConnection(_postgresConnection);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand("SELECT * FROM stores", conn);
        using var reader = await cmd.ExecuteReaderAsync();

        var stores = new List<Store>();
        while (await reader.ReadAsync())
        {
            var store = new Store
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                // Map other fields...
            };
            stores.Add(store);
        }

        if (stores.Any())
        {
            await storesCollection.InsertManyAsync(stores);
            Console.WriteLine($"Migrated {stores.Count} stores");
        }
    }

    private async Task MigrateMediaAsync()
    {
        // Similar implementation for media
    }
}
```

#### 4.2 Migration Strategy

**Option A: Big Bang (Downtime Required)**
1. Stop application
2. Run migration tool
3. Switch connection strings to MongoDB
4. Start application

**Option B: Gradual Migration (Zero Downtime)**
1. Deploy application with dual-write (write to both databases)
2. Run background migration tool
3. Verify data consistency
4. Switch reads to MongoDB
5. Remove PostgreSQL

**Recommendation:** Option A for simplicity (acceptable downtime for initial deployment).

### Phase 5: Testing and Validation (Week 5)

#### 5.1 Update Tests

**Update unit tests to use MongoDB Test Containers:**

```csharp
using Testcontainers.MongoDb;

public class MongoDbRepositoryTests : IAsyncLifetime
{
    private MongoDbContainer _mongoContainer = null!;
    private MongoDbContext _context = null!;

    public async Task InitializeAsync()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .Build();

        await _mongoContainer.StartAsync();

        var settings = Options.Create(new MongoDbSettings
        {
            ConnectionString = _mongoContainer.GetConnectionString(),
            DatabaseName = "test"
        });

        _context = new MongoDbContext(settings);
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddStore_ShouldPersistSuccessfully()
    {
        // Arrange
        var repository = new MongoStoreRepository(_context);
        var store = Store.Create("Test Store", address, Guid.NewGuid());

        // Act
        var result = await repository.AddAsync(store);

        // Assert
        var retrieved = await repository.GetByIdAsync(result.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Store", retrieved.Name);
    }
}
```

**Add NuGet Package:**
```xml
<PackageReference Include="Testcontainers.MongoDb" Version="3.7.0" />
```

#### 5.2 Performance Testing

**Create performance comparison:**

```csharp
[Fact]
public async Task Performance_InsertThousandStores()
{
    var stopwatch = Stopwatch.StartNew();
    
    var stores = Enumerable.Range(1, 1000)
        .Select(i => Store.Create($"Store {i}", address, Guid.NewGuid()))
        .ToList();

    foreach (var store in stores)
    {
        await _repository.AddAsync(store);
    }

    stopwatch.Stop();
    Console.WriteLine($"Inserted 1000 stores in {stopwatch.ElapsedMilliseconds}ms");
    
    Assert.True(stopwatch.ElapsedMilliseconds < 5000); // Should be fast
}
```

---

## Docker Deployment Configuration

### docker-compose.yml

```yaml
version: '3.8'

services:
  mongodb:
    image: mongo:7.0
    container_name: thriftmedia-mongodb
    restart: unless-stopped
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: ${MONGO_PASSWORD}
      MONGO_INITDB_DATABASE: thriftmediadb
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
      - mongodb_config:/data/configdb
    networks:
      - thriftmedia-network
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 10s
      timeout: 5s
      retries: 5

  mongo-express:
    image: mongo-express:latest
    container_name: thriftmedia-mongo-express
    restart: unless-stopped
    ports:
      - "8081:8081"
    environment:
      ME_CONFIG_MONGODB_ADMINUSERNAME: admin
      ME_CONFIG_MONGODB_ADMINPASSWORD: ${MONGO_PASSWORD}
      ME_CONFIG_MONGODB_URL: mongodb://admin:${MONGO_PASSWORD}@mongodb:27017/
    depends_on:
      - mongodb
    networks:
      - thriftmedia-network

  thriftmedia-api:
    image: thriftmedia-api:latest
    container_name: thriftmedia-api
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      MongoDbSettings__ConnectionString: mongodb://admin:${MONGO_PASSWORD}@mongodb:27017/thriftmediadb?authSource=admin
      MongoDbSettings__DatabaseName: thriftmediadb
    depends_on:
      mongodb:
        condition: service_healthy
    networks:
      - thriftmedia-network

volumes:
  mongodb_data:
  mongodb_config:

networks:
  thriftmedia-network:
    driver: bridge
```

### .env file

```env
MONGO_PASSWORD=YourSecurePasswordHere
```

### Deployment Commands

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f mongodb

# Stop services
docker-compose down

# Stop and remove volumes (CAUTION: deletes data)
docker-compose down -v
```

---

## Cost Comparison

### PostgreSQL (Container)

| Resource | Cost |
|----------|------|
| Docker image | Free |
| RAM usage | ~200-500 MB |
| Disk space | Varies by data |
| Licensing | Free (open source) |
| **Total Monthly Cost** | **$0** |

### MongoDB (Container)

| Resource | Cost |
|----------|------|
| Docker image | Free |
| RAM usage | ~200-400 MB |
| Disk space | Varies by data (similar to PostgreSQL) |
| Licensing | Free (open source) |
| **Total Monthly Cost** | **$0** |

**Conclusion:** Both are free when self-hosted. MongoDB may use slightly less memory for document-heavy workloads, but the difference is minimal for your scale.

---

## Updated DEVELOPMENT-RECOMMENDATIONS.md Changes

### Phase 1: Architecture Consolidation (Updated)

**Week 1:**
- [x] ~~Delete ThriftMedia.Data project~~ (Still do this)
- [x] **Remove EF Core and PostgreSQL dependencies**
- [x] **Add MongoDB.Driver NuGet packages**
- [x] **Create MongoDbContext**
- [x] **Update Domain entities with MongoDB attributes**

**Week 2:**
- [x] **Implement MongoDB repositories**
- [x] **Create indexes for performance**
- [x] Update API controllers to use IMediator (unchanged)
- [x] **Update Application layer handlers for MongoDB**
- [x] Add integration tests with MongoDB TestContainers

---

## Pros and Cons

### Advantages of MongoDB for ThriftMedia

✅ **Flexible Schema**: Easy to add new fields without migrations  
✅ **Document Model**: Natural fit for media items with varying properties  
✅ **Simple Deployment**: Single Docker container  
✅ **Horizontal Scalability**: Easy to add sharding later if needed  
✅ **JSON-Like Documents**: Natural mapping from C# objects  
✅ **Good Query Performance**: For both simple lookups and complex queries  
✅ **No Complex Migrations**: Schema changes don't require migration scripts  

### Disadvantages to Consider

❌ **No Joins**: Have to denormalize data or do application-level joins  
❌ **Eventual Consistency**: Replica sets have slight lag (not an issue for single server)  
❌ **Larger Storage**: Documents can be larger than normalized relational data  
❌ **Learning Curve**: Team needs to learn MongoDB query language  
❌ **Less Strict**: No foreign key constraints (enforced in application layer)  

### Mitigation Strategies

1. **No Joins**: Acceptable for media inventory - most queries are single-entity
2. **Consistency**: Single-server deployment doesn't have this issue
3. **Storage**: Modern disks are cheap; flexibility is worth it
4. **Learning Curve**: MongoDB.Driver API is intuitive for .NET developers
5. **Constraints**: Enforce via domain entities and validation

---

## Resource Requirements

### MongoDB System Requirements

**Minimum (Development):**
- RAM: 1 GB
- CPU: 1 core
- Disk: 10 GB

**Recommended (Production):**
- RAM: 4-8 GB
- CPU: 2-4 cores
- Disk: 50-100 GB SSD
- Network: 1 Gbps

**Linux Server Sizing (Running MongoDB + App):**
- **Small (< 1,000 stores)**: 4 GB RAM, 2 cores, 50 GB disk
- **Medium (1,000-10,000 stores)**: 8 GB RAM, 4 cores, 100 GB disk
- **Large (10,000+ stores)**: 16+ GB RAM, 8+ cores, 200+ GB disk

---

## Migration Checklist

### Pre-Migration
- [ ] Review current PostgreSQL schema and data
- [ ] Identify all queries and access patterns
- [ ] Design MongoDB collections and indexes
- [ ] Create backup of PostgreSQL data

### Implementation
- [ ] Add MongoDB dependencies
- [ ] Create MongoDbContext and settings
- [ ] Implement repositories with MongoDB
- [ ] Update Domain entities with MongoDB attributes
- [ ] Create indexes for performance
- [ ] Update Application layer handlers
- [ ] Update API service registration

### Testing
- [ ] Write unit tests with MongoDB TestContainers
- [ ] Test all CRUD operations
- [ ] Test complex queries (search, filter, location)
- [ ] Performance testing vs PostgreSQL
- [ ] Load testing with realistic data volume

### Migration
- [ ] Set up MongoDB container on Linux server
- [ ] Run data migration tool
- [ ] Verify data integrity
- [ ] Update connection strings in production
- [ ] Deploy updated application

### Post-Migration
- [ ] Monitor application performance
- [ ] Monitor MongoDB resource usage
- [ ] Verify all features working
- [ ] Update documentation
- [ ] Train team on MongoDB operations

---

## Rollback Plan

If issues arise after migration:

1. **Immediate Rollback (< 24 hours):**
   - Keep PostgreSQL container running during migration
   - Switch connection strings back to PostgreSQL
   - Restart application

2. **Data Recovery:**
   - Restore from PostgreSQL backup
   - Re-sync any changes made during MongoDB period

3. **Phased Rollback:**
   - Enable dual-write mode (write to both databases)
   - Switch reads back to PostgreSQL
   - Investigate and fix MongoDB issues
   - Retry migration when ready

---

## Maintenance and Monitoring

### Daily Tasks
- Monitor MongoDB logs for errors
- Check disk space usage
- Verify backups completed

### Weekly Tasks
- Review slow query logs
- Analyze index usage
- Check replication lag (if using replica set)

### Monthly Tasks
- Update MongoDB version (patch releases)
- Review and optimize indexes
- Capacity planning review

### Backup Strategy

```bash
#!/bin/bash
# MongoDB backup script

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backups/mongodb"

# Create backup
docker exec thriftmedia-mongodb mongodump \
    --username admin \
    --password $MONGO_PASSWORD \
    --authenticationDatabase admin \
    --out /data/backup_$TIMESTAMP

# Copy to host
docker cp thriftmedia-mongodb:/data/backup_$TIMESTAMP $BACKUP_DIR/

# Compress
tar -czf $BACKUP_DIR/backup_$TIMESTAMP.tar.gz $BACKUP_DIR/backup_$TIMESTAMP
rm -rf $BACKUP_DIR/backup_$TIMESTAMP

# Keep only last 7 days
find $BACKUP_DIR -name "*.tar.gz" -mtime +7 -delete

echo "Backup completed: backup_$TIMESTAMP.tar.gz"
```

---

## Next Steps

1. **Review this plan with the team**
2. **Decide on migration timeline**
3. **Set up development environment with MongoDB**
4. **Start Phase 1 implementation**
5. **Test thoroughly before production deployment**

---

**Document Version:** 1.0  
**Last Updated:** 2026-02-25  
**Author:** GitHub Copilot CLI  
**Status:** Ready for Implementation
