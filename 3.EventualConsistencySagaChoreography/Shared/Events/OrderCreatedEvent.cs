using System;
using Shared.Messages;

namespace Shared.Events;

public class OrderCreatedEvent
{   
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public List<OrderItemMessage> OrderItems { get; set; }
    public decimal TotalPrice { get; set; }
}
