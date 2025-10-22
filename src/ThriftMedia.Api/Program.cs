
using Microsoft.EntityFrameworkCore;
using ThriftMedia.Data.Models;

var builder = WebApplication.CreateBuilder(args);

// Connect to the database
var connectionString = builder.Configuration.GetConnectionString("ThriftMediaDb");
builder.Services.AddDbContext<ThriftMediaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
