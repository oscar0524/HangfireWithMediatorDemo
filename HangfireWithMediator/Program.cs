using HangfireWithMediator.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddServiceDefault();
builder.Services.AddHangfireDefault();

var app = builder.Build();
app.UseHangfireDefault();

app.Run();