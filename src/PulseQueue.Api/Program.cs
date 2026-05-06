using PulseQueue.Api.Services;
using PulseQueue.Infrastructure;
using PulseQueue.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddPulseQueueInfrastructure(builder.Configuration);
builder.Services.AddScoped<IJobService, JobService>();

var app = builder.Build();

await app.Services.EnsureDatabaseCreatedAsync();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
