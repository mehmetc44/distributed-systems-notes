using System;
using MassTransit;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
{
    readonly Order.API.Context.OrderApiDBContext _orderDbContext;
    public PaymentCompletedEventConsumer(Order.API.Context.OrderApiDBContext orderDbContext)
    {
        _orderDbContext = orderDbContext;
    }
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        Order.API.Models.Entites.Order order = await _orderDbContext.Orders.FindAsync(context.Message.OrderId);
        order.OrderStatus = Order.API.Models.Enums.OrderStatus.Completed;

        await _orderDbContext.SaveChangesAsync();
    }
}
