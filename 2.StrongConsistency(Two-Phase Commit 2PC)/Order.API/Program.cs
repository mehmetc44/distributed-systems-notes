var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();
app.MapGet("/ready", () =>
{
    Console.WriteLine("Order.API is ready");
    return true;
});
app.MapGet("/commit", () =>
{
    Console.WriteLine("Order.API is committed");
    return true;
});
app.MapGet("/rollback", () =>
{
    Console.WriteLine("Order.API is rolled back");
    return true;
});


app.Run();
