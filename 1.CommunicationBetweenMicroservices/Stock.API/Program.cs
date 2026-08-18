using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.RabbitMQSetings;
using Stock.API.Services;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<Stock.API.Consumers.OrderCreatedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(Environment.GetEnvironmentVariable("COMMUNICATION_BETWEEN_STOCK_API_RABBITMQ"));
        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e =>
        {
            e.ConfigureConsumer<Stock.API.Consumers.OrderCreatedEventConsumer>(context);
        });
    });
});

using IServiceScope? scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDBService mongoDBService = scope.ServiceProvider.GetService<MongoDBService>()!;
var collection = mongoDBService.GetCollection<Stock.API.Models.Entites.Stock>();
if (collection.CountDocuments(FilterDefinition<Stock.API.Models.Entites.Stock>.Empty) == 0)
{
    await collection.InsertOneAsync(new () { Id = Guid.NewGuid(), ProductId = Guid.Parse("00000000-0000-0000-0000-000000000000"), Quantity = 100 });
    await collection.InsertOneAsync(new () { Id = Guid.NewGuid(), ProductId = Guid.Parse("00000000-0000-0000-0000-000000000001"), Quantity = 200 });
    await collection.InsertOneAsync(new () { Id = Guid.NewGuid(), ProductId = Guid.Parse("00000000-0000-0000-0000-000000000002"), Quantity = 300 });
}

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
