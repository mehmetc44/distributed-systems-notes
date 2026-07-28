using System;
using MassTransit;
using Shared.Events.Common;

namespace Order.API.Consumers;

public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
{
     readonly Order.API.Context.OrderApiDBContext _orderDbContext;
    public PaymentFailedEventConsumer(Order.API.Context.OrderApiDBContext orderDbContext)
    {
        _orderDbContext = orderDbContext;
    }
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        Order.API.Models.Entites.Order order = await _orderDbContext.Orders.FindAsync(context.Message.OrderId);
        order.OrderStatus = Order.API.Models.Enums.OrderStatus.Failed;

        await _orderDbContext.SaveChangesAsync();
    }
}
