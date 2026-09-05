using MassTransit;
using Order.Outbox.Table.Publisher.Service.Jobs;
using Quartz;
using Shared;

EnvLoader.Load();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(Environment.GetEnvironmentVariable("INBOX_OUTBOX_RABBITMQ") ?? builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddQuartz(configurator =>
{
    JobKey jobKey = new("OrderOutboxPublishJob");
    configurator.AddJob<OrderOutboxPublishJob>(options => options.WithIdentity(jobKey));

    TriggerKey triggerKey = new("OrderOutboxPublishTrigger");
    configurator.AddTrigger(options => options.ForJob(jobKey)
    .WithIdentity(triggerKey)
    .StartAt(DateTime.UtcNow)
    .WithSimpleSchedule(builder => builder
        .WithIntervalInSeconds(5)
        .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();
