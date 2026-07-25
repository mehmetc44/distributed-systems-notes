using System;
using MongoDB.Driver;

namespace Stock.API.Services;

public class MongoDBService
{
    readonly IMongoDatabase _database;

    public MongoDBService(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("StockAPIMongoConnectionString"));
        _database = client.GetDatabase("StockDb");
    }

}
