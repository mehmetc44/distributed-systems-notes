using Coordinator.Abstraction;
using Coordinator.Services;
using Microsoft.EntityFrameworkCore;
using System.IO;

LoadEnv();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Coordinator.Context.TwoPhaseCommitContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("STRONG_CONSISTENCY_COORDINATOR_SQLITE_CONNECTION_STRING")
        ?? builder.Configuration.GetConnectionString("SQLiteConnectionString");
    options.UseSqlite(connectionString);
});
builder.Services.AddTransient<ITransactionService, TransactionService>();

builder.Services.AddHttpClient("OrderAPI", client =>{client.BaseAddress = new Uri("http://localhost:5220");});
builder.Services.AddHttpClient("PaymentAPI", client =>{client.BaseAddress = new Uri("http://localhost:5115");});
builder.Services.AddHttpClient("StockAPI", client =>{client.BaseAddress = new Uri("http://localhost:5201");});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/create-order-transaction", async (ITransactionService transactionService) =>
{
    //Phase 1 - Prepare
    var transactionId = await transactionService.CreateTransactionAsync();
    await transactionService.PrepareServicesAsync(transactionId);
    bool transactionState = await transactionService.CheckReadyServicesAsync(transactionId);

    if (transactionState)
    {
        //Phase 2 - Commit
        await transactionService.CommitAsync(transactionId);
        transactionState = await transactionService.CheckTransactionStateServicesAsync(transactionId);
    }

    if (!transactionState)
        await transactionService.RollbackAsync(transactionId);
});

app.Run();

void LoadEnv()
{
    var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (currentDir != null)
    {
        var filePath = Path.Combine(currentDir.FullName, ".env");
        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                    continue;

                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
            break;
        }
        currentDir = currentDir.Parent;
    }
}
