using Microsoft.EntityFrameworkCore;
using ThriftMedia.Data.Models;

namespace ThriftMedia.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly ThriftMediaDbContext _context;
    private static readonly Random _random = new();

    // Predefined data for variety
    private static readonly string[] StoreNames =
    {
        "Vintage Vault", "Retro Records & Books", "Nostalgia Corner",
        "The Second Spin", "Classic Collections", "Memory Lane Media",
        "Throwback Treasures", "Old School Finds", "Yesterday's Media",
        "Timeless Thrift"
    };

    private static readonly string[] Streets =
    {
        "123 Main St", "456 Oak Ave", "789 Elm Blvd", "321 Maple Dr",
        "654 Pine St", "987 Cedar Ln", "147 Birch Way", "258 Walnut Ct",
        "369 Spruce Rd", "741 Ash Pl"
    };

    private static readonly string[] Cities =
    {
        "Portland", "Austin", "Seattle", "Denver", "Nashville",
        "Minneapolis", "Phoenix", "San Diego", "Boston", "Chicago"
    };

    private static readonly string[] States =
    {
        "OR", "TX", "WA", "CO", "TN", "MN", "AZ", "CA", "MA", "IL"
    };

    private static readonly string[] MediaTypes =
    {
        "book", "video", "cdrom", "vinyl-record", "eight-track",
        "cassette", "dvd", "blu-ray", "magazine", "comic", "other"
    };

    public DatabaseSeeder(ThriftMediaDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if we already have data
        if (await _context.Stores.AnyAsync())
        {
            return; // Already seeded
        }

        var createdBy = "System-Seeder";
        var now = DateTime.UtcNow;

        var stores = new List<Store>();

        // Create 10 stores
        for (int i = 0; i < 10; i++)
        {
            var store = new Store
            {
                Name = StoreNames[i],
                Address = new Address
                {
                    Street = Streets[i],
                    City = Cities[i],
                    State = States[i],
                    ZipCode = _random.Next(10000, 99999).ToString(),
                    Country = "USA"
                },
                BusinessLicenseImageUri = $"https://example.com/licenses/store-{i + 1}.jpg",
                ExplicitContentFlagged = false,
                CreatedBy = createdBy,
                CreatedAt = now,
                UpdatedBy = null,
                UpdatedAt = null
            };

            stores.Add(store);
        }

        // Add stores to context
        await _context.Stores.AddRangeAsync(stores);
        await _context.SaveChangesAsync();

        // Now create media for each store
        var allMedia = new List<Media>();

        foreach (var store in stores)
        {
            // Random number of media items between 10 and 100
            int mediaCount = _random.Next(10, 101);

            for (int i = 0; i < mediaCount; i++)
            {
                var mediaType = MediaTypes[_random.Next(MediaTypes.Length)];

                var media = new Media
                {
                    StoreId = store.Id,
                    Type = mediaType,
                    ImageUri = $"https://example.com/media/store-{store.Id}-item-{i + 1}.jpg",
                    OcrPayloadJson = GenerateOcrJson(mediaType, i),
                    IsExplicitContent = false,
                    CreatedBy = createdBy,
                    CreatedAt = now.AddMinutes(_random.Next(-10000, 0)), // Vary creation times
                    UpdatedBy = null,
                    UpdatedAt = null
                };

                allMedia.Add(media);
            }
        }

        // Add all media in batch
        await _context.MediaItems.AddRangeAsync(allMedia);
        await _context.SaveChangesAsync();
    }

    private static string GenerateOcrJson(string mediaType, int index)
    {
        // Generate realistic-looking OCR data based on media type
        return mediaType switch
        {
            "book" => $$"""{"title": "Book Title {{index}}", "author": "Author Name", "isbn": "978-{{_random.Next(1000000000, 2000000000)}}", "publisher": "Publisher {{_random.Next(1, 10)}}", "year": {{_random.Next(1950, 2024)}}}""",
            "video" => $$"""{"title": "Video {{index}}", "format": "VHS", "studio": "Studio {{_random.Next(1, 5)}}", "year": {{_random.Next(1980, 2010)}}, "rating": "PG"}""",
            "dvd" or "blu-ray" => $$"""{"title": "Movie {{index}}", "director": "Director Name", "year": {{_random.Next(1990, 2024)}}, "rating": "PG-13", "runtime": "{{_random.Next(90, 180)}} min"}""",
            "vinyl-record" or "cassette" or "eight-track" => $$"""{"artist": "Artist {{index}}", "album": "Album Title", "year": {{_random.Next(1960, 2000)}}, "genre": "Rock", "label": "Label {{_random.Next(1, 8)}}"}""",
            "magazine" => $$"""{"title": "Magazine {{index}}", "issue": "Vol {{_random.Next(1, 50)}} No {{_random.Next(1, 12)}}", "month": "{{GetRandomMonth()}}", "year": {{_random.Next(1980, 2023)}}}""",
            "comic" => $$"""{"title": "Comic {{index}}", "issue": "#{{_random.Next(1, 500)}}", "publisher": "Publisher {{_random.Next(1, 5)}}", "year": {{_random.Next(1970, 2023)}}, "grade": "VF"}""",
            _ => $$"""{"title": "Item {{index}}", "description": "Generic media item", "condition": "Good"}"""
        };
    }

    private static string GetRandomMonth()
    {
        string[] months = { "January", "February", "March", "April", "May", "June",
                           "July", "August", "September", "October", "November", "December" };
        return months[_random.Next(months.Length)];
    }
}
