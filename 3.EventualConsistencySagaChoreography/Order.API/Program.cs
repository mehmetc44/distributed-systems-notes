using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models.Contexts;
using Order.API.ViewModels;
using Shared;
using Shared.Events;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderAPIDbContext>(options =>
{
    options.UseSqlite(Environment.GetEnvironmentVariable("EVENTUAL_CHOREOGRAPHY_ORDER_API_CONNECTION_STRING"));
});

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<Order.API.Consumers.PaymentCompletedConsumer>();
    configurator.AddConsumer<Order.API.Consumers.PaymentFailedEventConsumer>();
    configurator.AddConsumer<Order.API.Consumers.StockNotReservedEventConsumer>();

    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            Environment.GetEnvironmentVariable(
                "EVENTUAL_CHOREOGRAPHY_ORDER_API_RABBITMQ"
            )
        );

        cfg.ReceiveEndpoint(
            RabbitMQSettings.Order_PaymentCompletedEventQueue,
            e =>
            {
                e.ConfigureConsumer<Order.API.Consumers.PaymentCompletedConsumer>(context);
            }
        );

        cfg.ReceiveEndpoint(
            RabbitMQSettings.Order_PaymentFailedEventQueue,
            e =>
            {
                e.ConfigureConsumer<Order.API.Consumers.PaymentFailedEventConsumer>(context);
            }
        );
        cfg.ReceiveEndpoint(
            RabbitMQSettings.Payment_StockNotReservedEventQueue,
            e =>
            {
                e.ConfigureConsumer<Order.API.Consumers.StockNotReservedEventConsumer>(context);
            }
        );
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-order", async (CreateOrderVM model, OrderAPIDbContext context, IPublishEndpoint publishEndpoint) =>
{
    Order.API.Models.Order order = new()
    {
        BuyerId = Guid.TryParse(model.BuyerId, out Guid _buyerId) ? _buyerId : Guid.NewGuid(),
        OrderItems = model.OrderItems.Select(oi => new Order.API.Models.OrderItem()
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = oi.ProductId
        }).ToList(),
        OrderStatus = Order.API.Enums.OrderStatus.Suspend,
        CreatedDate = DateTime.UtcNow,
        TotalPrice = model.OrderItems.Sum(oi => oi.Price * oi.Count)
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    OrderCreatedEvent orderCreatedEvent = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = order.TotalPrice,
        OrderItems = order.OrderItems.Select(oi => new Shared.Messages.OrderItemMessage()
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = oi.ProductId,
        }).ToList()
    };
    await publishEndpoint.Publish(orderCreatedEvent);
});
app.UseHttpsRedirection();

app.Run();
