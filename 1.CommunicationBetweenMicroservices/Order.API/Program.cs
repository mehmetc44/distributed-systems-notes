using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Context;
using Shared;
using Shared.RabbitMQSetings;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderApiDBContext>(options =>
{
    options.UseSqlite(
        Environment.GetEnvironmentVariable("COMMUNICATION_BETWEEN_ORDER_API_CONNECTION_STRING"));
});
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentCompletedEventConsumer>();
    x.AddConsumer<StockNotReservedConsumer>();
    x.AddConsumer<PaymentFailedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(Environment.GetEnvironmentVariable("COMMUNICATION_BETWEEN_ORDER_API_RABBITMQ"));
        cfg.ReceiveEndpoint(RabbitMQSettings.Order_PaymentCompletedEventQueue, e =>{
            e.ConfigureConsumer<PaymentCompletedEventConsumer>(context);   
        });
        cfg.ReceiveEndpoint(RabbitMQSettings.Order_StockNotReservedEventQueue, e =>{
            e.ConfigureConsumer<StockNotReservedConsumer>(context);   
        });
        cfg.ReceiveEndpoint(RabbitMQSettings.Order_PaymentFailedEventQueue, e =>{
            e.ConfigureConsumer<PaymentFailedEventConsumer>(context);   
        });
    });
});
var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();   

app.Run();