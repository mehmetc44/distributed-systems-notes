using System;
using MassTransit;
using Shared.Events;
using Shared.Events.Common;

namespace Payment.API.Consumers;

public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
{
    IPublishEndpoint _publishEndpoint;

    public StockReservedEventConsumer(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    public Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        //Ödeme İşlemleri
        Console.WriteLine("Ödeme tamamlandı event'i publish edilecek");
        if (true)
        {
            PaymentCompletedEvent paymentCompletedEvent = new()
            {
                OrderId = context.Message.OrderId
            };
            _publishEndpoint.Publish(paymentCompletedEvent);
        }
        else
        {
            PaymentFailedEvent paymentFailedEvent = new()
            {
                OrderId = context.Message.OrderId,
                Message = "Ödeme işlemi başarısız oldu."
            };
            _publishEndpoint.Publish(paymentFailedEvent);
        }
        return Task.CompletedTask;
    }

}
