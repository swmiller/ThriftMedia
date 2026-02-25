
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ThriftMedia.Data.Models;
using ThriftMedia.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Connect to the database
builder.Services.AddDbContext<ThriftMediaDbContext>(options =>

// Choose between SQL and NoSQL infrastructure based on configuration
var useNoSql = builder.Configuration.GetValue<bool>("UseNoSql");
if (useNoSql)
{
    // Register NoSQL infrastructure
    builder.Services.AddNoSqlInfrastructure(builder.Configuration);
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("ThriftMediaDb");
    builder.Services.AddDbContext<ThriftMediaDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize database (migrations and seeding)
await app.InitializeDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Register media endpoints
app.MapMediaEndpoints();

// Register store owner endpoints
app.MapStoreOwnerEndpoints();

app.Run();
