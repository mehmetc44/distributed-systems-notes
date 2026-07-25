using System;
using MassTransit;
using Shared.Events;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        Console.WriteLine($"OrderCreatedEvent consumed: OrderId: {context.Message.OrderId}");
        return Task.CompletedTask;
    }
}
