using System;

namespace ThriftMedia.Tests;

public class ApiTests : AspireTestBase
{
    private const string ThriftMediaApiResourceName = "api";
    private const string MediaEndpoint = "/media";

    /// <summary>
    /// Tests that the GET /media endpoint returns HTTP 200 OK.
    /// </summary>
    [Fact]
    public async Task GetAllMedia_ReturnsOk()
    {
        // Arrange: Set up the test environment and get an HttpClient for the thriftmediaapi service
        var setupResult = await SetupTestEnvironment(ThriftMediaApiResourceName);
        var httpClient = setupResult.client;
        await using var app = setupResult.app; // Ensures the app is disposed after the test

        // Act: Send a GET request to the /media endpoint
        var response = await httpClient.GetAsync(MediaEndpoint);

        // Assert: Verify that the response status code is 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Thrift Store API CRUD Test List (TDD)
    //
    // 1. Create Thrift Store
    //    - Should return 201 Created and the new store's Id when valid data is provided.
    //    - Should return 400 Bad Request when required fields are missing or invalid.
    //
    // 2. Retrieve Thrift Store
    //    - Should return 200 OK and store details when a valid Id is provided.
    //    - Should return 404 Not Found when the store does not exist.
    //    - Should return a list of stores for GET all.
    //
    // 3. Update Thrift Store
    //    - Should return 200 OK and updated store details when valid data is provided.
    //    - Should return 400 Bad Request when attempting to update the Id or when required fields are missing/invalid.
    //    - Should return 404 Not Found when updating a non-existent store.
    //
    // 4. Delete Thrift Store
    //    - Should return 204 No Content when a store is successfully deleted.
    //    - Should delete associated media records.
    //    - Should return 404 Not Found when deleting a non-existent store.

}
