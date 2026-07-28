using System;
using Shared.Events.Common;
using Shared.Messages;

namespace Shared.Events;

public class StockNotReservedEvent : IEvent
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public string Message { get; set; }
}
