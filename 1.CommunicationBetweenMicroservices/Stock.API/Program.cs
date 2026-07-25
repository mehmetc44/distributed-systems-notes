using MassTransit;
using Shared.RabbitMQSetings;
using Stock.API.Services;

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
        cfg.Host(builder.Configuration["RabbitMq"]);
        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e =>
        {
            e.ConfigureConsumer<Stock.API.Consumers.OrderCreatedEventConsumer>(context);
        });
    });
});

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

