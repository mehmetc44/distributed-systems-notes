using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Context;
using Order.API.ViewModels;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderApiDBContext _context;

        public OrdersController(OrderApiDBContext context)
        {
            _context = context;
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
            return Ok();
        }
    }
}
