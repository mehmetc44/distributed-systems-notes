using System;
using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Shared.Messages;
using Shared.RabbitMQSetings;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    IMongoCollection<Stock.API.Models.Entites.Stock> _stockCollection;
    IPublishEndpoint _publishEndpoint;
    ISendEndpointProvider _sendEndpointProvider;

    public OrderCreatedEventConsumer(MongoDBService _mongoDbService, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
    {
        _stockCollection = _mongoDbService.GetCollection<Stock.API.Models.Entites.Stock>();
        _sendEndpointProvider = sendEndpointProvider;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        List<bool> stockResult = new();

        foreach (var item in context.Message.OrderItems)
        {
            bool exists = await _stockCollection
                .Find(x => x.ProductId == item.ProductId &&
                           x.Quantity >= item.Quantity)
                .AnyAsync();

            stockResult.Add(exists);
        }
        if (stockResult.TrueForAll(sr => sr.Equals(true)))
        {
            foreach(OrderItemMessage orderItemMessage in context.Message.OrderItems)
            {
                var stock = await _stockCollection
                    .Find(x => x.ProductId == orderItemMessage.ProductId)
                    .FirstOrDefaultAsync();

                stock.Quantity -= orderItemMessage.Quantity;

                await _stockCollection.ReplaceOneAsync(x => x.Id == stock.Id, stock);
            }
            StockReservedEvent stockReservedEvent = new()
            {
                OrderId = context.Message.OrderId,
                BuyerId = context.Message.BuyerId,
                OrderItems = context.Message.OrderItems
            };
            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));
            await sendEndpoint.Send(stockReservedEvent);
        }
        else
        {
            StockNotReservedEvent stockNotReservedEvent = new()
            {
                OrderId = context.Message.OrderId,
                BuyerId = context.Message.BuyerId,
                Message = "Stok yetersiz."
            };
            await _publishEndpoint.Publish(stockNotReservedEvent);
        }
        return;
    }
}
