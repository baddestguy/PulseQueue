using PulseQueue.Worker;
using PulseQueue.Infrastructure;
using PulseQueue.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddPulseQueueInfrastructure(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.Services.EnsureDatabaseCreatedAsync();
host.Run();
