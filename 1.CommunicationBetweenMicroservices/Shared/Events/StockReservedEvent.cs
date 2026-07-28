using System;
using Shared.Events.Common;
using Shared.Messages;

namespace Shared.Events;

public class StockReservedEvent : IEvent
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public List<OrderItemMessage> OrderItems { get; set; }
}
