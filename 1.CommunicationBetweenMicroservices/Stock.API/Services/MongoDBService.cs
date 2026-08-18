using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDBService
{
    readonly IMongoDatabase _database;

    public MongoDBService(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_BETWEEN_STOCK_API_MONGO_CONNECTION_STRING")
            ?? configuration.GetConnectionString("StockAPIMongoConnectionString");
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("StockDb");
    }
    public IMongoCollection<T> GetCollection<T>()=> _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
}
