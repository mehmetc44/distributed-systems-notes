using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shared;

namespace Order.API.Models.Contexts;

public class OrderAPIDbContextFactory : IDesignTimeDbContextFactory<OrderAPIDbContext>
{
    public OrderAPIDbContext CreateDbContext(string[] args)
    {
        EnvLoader.Load();
        var optionsBuilder = new DbContextOptionsBuilder<OrderAPIDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("EVENTUAL_CHOREOGRAPHY_ORDER_API_CONNECTION_STRING")
            ?? "Data Source=Data/order.db";
        optionsBuilder.UseSqlite(connectionString);

        return new OrderAPIDbContext(optionsBuilder.Options);
    }
}
