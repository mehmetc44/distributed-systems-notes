using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Services;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MongoDBService>();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<Stock.API.Consumers.OrderCreatedEventConsumer>();
    configurator.AddConsumer<Stock.API.Consumers.PaymentFailedEventConsumer>();

    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(Environment.GetEnvironmentVariable("EVENTUAL_CHOREOGRAPHY_STOCK_API_RABBITMQ"));
        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e =>
        {
            e.ConfigureConsumer<Stock.API.Consumers.OrderCreatedEventConsumer>(context);
        });
        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_PaymentFailedEventQueue, e =>
        {
            e.ConfigureConsumer<Stock.API.Consumers.PaymentFailedEventConsumer>(context);
        });
    });
});

var app = builder.Build();

using IServiceScope scope = app.Services.CreateScope();
MongoDBService mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDBService>();
var stockCollection = mongoDbService.GetCollection<Stock.API.Models.Stock>();
if (!await stockCollection.Find(_ => true).AnyAsync())
{
    Console.WriteLine("MongoDB'ye örnek stok verileri ekleniyor...");
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 100 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 200 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 50 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 30 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid(), Count = 5 });
}

var stocks = await stockCollection.Find(_ => true).ToListAsync();
Console.WriteLine($"MongoDB Stok Sayısı: {stocks.Count}");
foreach (var s in stocks)
{
    Console.WriteLine($"[Stock] ProductId: {s.ProductId} | Count: {s.Count}");
}


app.Run();
