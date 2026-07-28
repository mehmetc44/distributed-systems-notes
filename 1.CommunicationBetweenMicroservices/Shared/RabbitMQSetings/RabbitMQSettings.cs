using System;

namespace Shared.RabbitMQSetings;

public static class RabbitMQSettings
{
    public const string Stock_OrderCreatedEventQueue = "order-created-event-stock-service";
    public const string Payment_StockReservedEventQueue = "payment-stock-reserved-event-queue";
    public const string Order_PaymentCompletedEventQueue = "payment-completed-event-order-api";
    public const string Order_StockNotReservedEventQueue = "stock-not-reserved-event-queue";
    public const string Order_PaymentFailedEventQueue = "payment-failed-event-queue";
}
