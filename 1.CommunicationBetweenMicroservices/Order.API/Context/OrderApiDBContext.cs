using System;
using Microsoft.EntityFrameworkCore;

namespace Order.API.Context;

public class OrderApiDBContext : DbContext
{
    public OrderApiDBContext(DbContextOptions<OrderApiDBContext> options) : base(options)
    {
    }

    public DbSet<Order.API.Models.Entites.Order> Orders { get; set; }
    public DbSet<Order.API.Models.Entites.OrderItem> OrderItems { get; set; }
}
