using MassTransit;
using Payment.API.Consumers;
using Shared;
using Shared.Events;
using Shared.RabbitMQSetings;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StockReservedEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(Environment.GetEnvironmentVariable("COMMUNICATION_BETWEEN_PAYMENT_API_RABBITMQ"));
        cfg.ReceiveEndpoint(RabbitMQSettings.Payment_StockReservedEventQueue, e =>
        {
            e.ConfigureConsumer<StockReservedEventConsumer>(context);
        });
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.Run();
