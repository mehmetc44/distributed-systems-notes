var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();
app.MapGet("/ready", () =>
{
    Console.WriteLine("Stock.API is ready");
    return true;
});
app.MapGet("/commit", () =>
{
    Console.WriteLine("Stock.API is committed");
    return true;
});
app.MapGet("/rollback", () =>
{
    Console.WriteLine("Stock.API is rolled back");
    return true;
});


app.Run();
