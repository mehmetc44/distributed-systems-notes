using System;
using MassTransit;
using Shared.Events;

namespace Order.API.Consumers;

public class StockNotReservedConsumer : IConsumer<StockNotReservedEvent>
{
    readonly Order.API.Context.OrderApiDBContext _orderDbContext;
    public StockNotReservedConsumer(Order.API.Context.OrderApiDBContext orderDbContext)
    {
        _orderDbContext = orderDbContext;
    }
    public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
    {
        Order.API.Models.Entites.Order order = await _orderDbContext.Orders.FindAsync(context.Message.OrderId);
        order.OrderStatus = Order.API.Models.Enums.OrderStatus.Failed;

        await _orderDbContext.SaveChangesAsync();
    }
}
