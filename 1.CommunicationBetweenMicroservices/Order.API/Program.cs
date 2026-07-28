using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Context;
using Shared.RabbitMQSetings;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderApiDBContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("OrderApiConnectionString"));
});
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentCompletedEventConsumer>();
    x.AddConsumer<StockNotReservedConsumer>();
    x.AddConsumer<PaymentFailedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq"]);
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