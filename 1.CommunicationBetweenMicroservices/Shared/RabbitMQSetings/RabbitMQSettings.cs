using System;

namespace Shared.RabbitMQSetings;

public static class RabbitMQSettings
{
    public const string Stock_OrderCreatedEventQueue = "order-created-event-stock-service";
    public const string Payment_StockReservedEventQueue = "payment-stock-reserved-event-queue";

}
