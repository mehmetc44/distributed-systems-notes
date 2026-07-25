using System;

namespace Order.API.ViewModels;

public class CreateOrderViewModel
{
    public Guid BuyerId { get; set; }
    public List<CreateOrderItemViewModel> OrderItems { get; set; }
}
public class CreateOrderItemViewModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
}
