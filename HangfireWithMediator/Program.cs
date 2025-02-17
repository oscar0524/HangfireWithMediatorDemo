using HangfireWithMediator.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMediatRDefault();
builder.Services.AddHangfireDefault();
builder.AddOpenTelemetryDefault();

var app = builder.Build();
app.UseHangfireDefault();

app.Run();