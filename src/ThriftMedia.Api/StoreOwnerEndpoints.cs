using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ThriftMedia.Application.Queries;
using ThriftMedia.Mediator;

public static class StoreOwnerEndpoints
{
    public static IEndpointRouteBuilder MapStoreOwnerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Get all stores
        endpoints.MapGet("/storeowners", async (IMediator mediator) =>
        {
            var stores = await mediator.Send(new GetAllStoresQuery());
            return Results.Ok(stores);
        });

        //// Get all store owners
        //endpoints.MapGet("/storeowners", async (ThriftMediaDbContext db) =>
        //    await db.StoreOwners.Include(s => s.MediaItems).ToListAsync());

        //// Get store owner by ID
        //endpoints.MapGet("/storeowners/{id:int}", async (int id, ThriftMediaDbContext db) =>
        //{
        //    var owner = await db.StoreOwners.Include(s => s.MediaItems).FirstOrDefaultAsync(s => s.Id == id);
        //    return owner is not null ? Results.Ok(owner) : Results.NotFound();
        //});

        //// Create new store owner
        //endpoints.MapPost("/storeowners", async (StoreOwner owner, ThriftMediaDbContext db) =>
        //{
        //    owner.CreatedAt = DateTime.UtcNow;
        //    db.StoreOwners.Add(owner);
        //    await db.SaveChangesAsync();
        //    return Results.Created($"/storeowners/{owner.Id}", owner);
        //});

        //// Update store owner
        //endpoints.MapPut("/storeowners/{id:int}", async (int id, StoreOwner updated, ThriftMediaDbContext db) =>
        //{
        //    var owner = await db.StoreOwners.FindAsync(id);
        //    if (owner is null) return Results.NotFound();

        //    owner.StoreName = updated.StoreName;
        //    owner.Address = updated.Address;
        //    owner.PhoneNumber = updated.PhoneNumber;
        //    owner.UpdatedBy = updated.UpdatedBy;
        //    owner.UpdatedAt = DateTime.UtcNow;
        //    await db.SaveChangesAsync();
        //    return Results.Ok(owner);
        //});

        //// Delete store owner
        //endpoints.MapDelete("/storeowners/{id:int}", async (int id, ThriftMediaDbContext db) =>
        //{
        //    var owner = await db.StoreOwners.FindAsync(id);
        //    if (owner is null) return Results.NotFound();
        //    db.StoreOwners.Remove(owner);
        //    await db.SaveChangesAsync();
        //    return Results.NoContent();
        //});

        return endpoints;
    }
}
