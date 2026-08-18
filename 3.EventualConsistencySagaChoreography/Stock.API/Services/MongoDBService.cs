using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDBService
{
    readonly IMongoDatabase _database;
    public MongoDBService(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("EVENTUAL_CHOREOGRAPHY_STOCK_API_CONNECTION_STRING")
            ?? configuration.GetConnectionString("MongoDB");
        MongoClient client = new(connectionString);
        _database = client.GetDatabase("stockdb");
    }
    public IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
}