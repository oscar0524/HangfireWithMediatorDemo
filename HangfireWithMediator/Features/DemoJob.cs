using System;
using HangfireWithMediator.Abstractions;

namespace HangfireWithMediator.Features;

public class DemoJob
{
    public class Request : IJobRequest
    {
        public string JobId { get; set; } = Guid.NewGuid().ToString();
        public string Cron { get; set; } = "*/5 * * * *";
    }

    public class Handler(ILogger<Handler> logger) : IJobHandler<Request>
    {        
        public Task Handle(Request request, CancellationToken cancellationToken)
        {
            logger.LogInformation($"DemoJob is running at {DateTime.Now}");
            return Task.CompletedTask;
        }      
    }

}
