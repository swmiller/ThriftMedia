using System.Threading.RateLimiting;
using Microsoft.Extensions.Hosting;
using ThriftMedia.Api.Auth;
using ThriftMedia.Api.Errors;
using ThriftMedia.Api.Features.Auth.ResolveSignIn;
using ThriftMedia.Api.Features.StoreAdmins.ReactivateStoreAdmin;
using ThriftMedia.Api.Features.StoreAdmins.SuspendStoreAdmin;
using ThriftMedia.Api.Features.StoreApplications.ApproveStoreApplication;
using ThriftMedia.Api.Features.StoreApplications.GetPendingStoreApplications;
using ThriftMedia.Api.Features.StoreApplications.RejectStoreApplication;
using ThriftMedia.Api.Features.StoreApplications.SubmitStoreApplication;
using ThriftMedia.Application;
using ThriftMedia.Infrastructure;
using ThriftMedia.Api.Features.Stores.ChangeStoreAddress;
using ThriftMedia.Api.Features.Stores.CreateStore;
using ThriftMedia.Api.Features.Stores.DeleteStore;
using ThriftMedia.Api.Features.Stores.GetAllStores;
using ThriftMedia.Api.Features.Stores.GetStoreById;
using ThriftMedia.Api.Features.Stores.RenameStore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Shared NpgsqlDataSource (pooling, health check, telemetry); connection string "ThriftMediaDb" is injected by Aspire.
builder.AddNpgsqlDataSource("ThriftMediaDb");

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddApiAuthentication();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// Throttle the public application form (per client IP). Behind a reverse proxy, configure forwarded headers so the real IP is used.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(SubmitStoreApplicationEndpoint.RateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseExceptionHandler();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// TODO: Register media endpoints (media search must be marked AllowAnonymous; everything else is deny-by-default)

// Auth and account management
app.MapResolveSignInEndpoint();
app.MapSubmitStoreApplicationEndpoint();
app.MapGetPendingStoreApplicationsEndpoint();
app.MapApproveStoreApplicationEndpoint();
app.MapRejectStoreApplicationEndpoint();
app.MapSuspendStoreAdminEndpoint();
app.MapReactivateStoreAdminEndpoint();


// Register store owner endpoints
app.MapGetAllStoresEndpoint();
app.MapGetStoreByIdEndpoint();
app.MapCreateStoreEndpoint();
app.MapRenameStoreEndpoint();
app.MapChangeStoreAddressEndpoint();
app.MapDeleteStoreEndpoint();

app.Run();

