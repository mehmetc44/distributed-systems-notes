using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Context;
using Order.API.ViewModels;
using Shared.Events;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderApiDBContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(OrderApiDBContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderViewModel model)
        {
         Order.API.Models.Entites.Order order = new Order.API.Models.Entites.Order()
            
            {
                Id = Guid.NewGuid(),
                TotalPrice = (decimal)model.OrderItems.Sum(x => x.Price * x.Quantity),
                CreatedDate = DateTime.Now,
                OrderStatus = Order.API.Models.Enums.OrderStatus.Complated,
                BuyerId = model.BuyerId,
                OrderItems = model.OrderItems.Select(x => new Order.API.Models.Entites.OrderItem()
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList(),
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent()
            {
                OrderId = order.Id,
                BuyerId = order.BuyerId,
                OrderItems = order.OrderItems.Select(x => new Shared.Messages.OrderItemMessage()
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                }).ToList()
            };
            await _publishEndpoint.Publish(orderCreatedEvent);
            return Ok();
        }
    }
}
