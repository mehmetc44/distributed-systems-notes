using System;
using MassTransit;

namespace Payment.API.Consumers;

public class StockReservedEventConsumer : IConsumer<StockReservedEventConsumer>
{
    public Task Consume(ConsumeContext<StockReservedEventConsumer> context)
    {
        throw new NotImplementedException();
    }
}
