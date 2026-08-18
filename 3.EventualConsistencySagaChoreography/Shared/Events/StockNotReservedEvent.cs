using System;
using Shared.Messages;

namespace Shared.Events;

public class StockNotReservedEvent
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public string Message { get; set; }
}
