using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ThriftMedia.Api.Features.Stores.ChangeStoreAddress;
using ThriftMedia.Api.Features.Stores.CreateStore;
using ThriftMedia.Api.Features.Stores.GetAllStores;
using ThriftMedia.Api.Features.Stores.GetStoreById;
using ThriftMedia.Api.Features.Stores.RenameStore;
using ThriftMedia.Infrastructure.DependencyInjection;
//using ThriftMedia.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register Application and Infrastructure layers
builder.Services.AddInfrastructure(builder.Configuration);

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

// TODO: Register media endpoints


// Register store owner endpoints
app.MapGetAllStoresEndpoint();
app.MapGetStoreByIdEndpoint();
app.MapCreateStoreEndpoint();
app.MapRenameStoreEndpoint();
app.MapChangeStoreAddressEndpoint();

app.Run();
