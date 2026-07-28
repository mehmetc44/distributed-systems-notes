using System;

namespace Shared.Events.Common;

public class PaymentFailedEvent
{
    public Guid OrderId { get; set; }
    public string Message { get; set; }
}
