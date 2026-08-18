using System;
using MassTransit;
using Shared.Events;
using Stock.API.Services;
using MongoDB.Driver;
namespace Stock.API.Consumers;

public class PaymentFailedEventConsumer(MongoDBService mongoDBService) : IConsumer<Shared.Events.PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var stocks = mongoDBService.GetCollection<Models.Stock>();
        foreach (var orderItem in context.Message.OrderItems)
        {
            var stockItem = await (await stocks.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
            if (stockItem != null)
            {
                stockItem.Count += orderItem.Count;
                stocks.ReplaceOne(s => s.Id == stockItem.Id, stockItem);
            }
        }
        {
            
        }
    }
}
