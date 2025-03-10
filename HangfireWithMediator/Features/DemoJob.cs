using System;
using HangfireWithMediator.Abstractions;

namespace HangfireWithMediator.Features;

public class DemoJob
{
    public class Request : IJobRequest
    {
        public string JobId { get; set; } = "DemoJob";
        public string Cron { get; set; } = "*/1 * * * *"; // 每 1 分鐘執行一次
    }

    public class Handler(ILogger<Handler> logger) : IJobHandler<Request>
    {
        public Task Handle(Request request, CancellationToken cancellationToken)
        {
            logger.LogInformation($"DemoJob is running at {DateTime.Now}");
            logger.LogWarning("This is a warning message.");
            logger.LogError("This is an error message.");
            return Task.CompletedTask;
        }
    }

}
