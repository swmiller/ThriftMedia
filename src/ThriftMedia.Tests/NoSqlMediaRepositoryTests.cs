using Xunit;
using MongoDB.Driver;
using ThriftMedia.Infrastructure.NoSql.Repositories;
using ThriftMedia.Domain.Entities;
using System.Threading.Tasks;
using System;

namespace ThriftMedia.Tests.NoSql
{
    public class NoSqlMediaRepositoryTests
    {
        private readonly IMongoDatabase _database;
        private readonly NoSqlMediaRepository _repository;

        public NoSqlMediaRepositoryTests()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _database = client.GetDatabase("ThriftMediaTestDb");
            _repository = new NoSqlMediaRepository(_database);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _repository.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        // Add more tests for NoSQL repository methods
    }
}
