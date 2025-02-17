using System;
using System.Diagnostics;
using System.Reflection;
using MediatR;

namespace HangfireWithMediator.Core.Extensions;

public static class MediatRExtension
{
    public class MediatRTelemetry
    {
        public const string ActivitySourceName = "Hangfire";
        public readonly ActivitySource ActivitySource = new ActivitySource(ActivitySourceName);
    }

    public class TelemetryPipelineBehavior<TRequest, TResponse>(
        MediatRTelemetry telemetry
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ActivitySource activitySource = telemetry.ActivitySource;
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var activityName = $"MediatR Request {typeof(TRequest).FullName?.Replace("+", ".")}";
            var parentContext = Activity.Current?.Context ?? default;
            using var activity = activitySource.StartActivity(activityName, ActivityKind.Client, parentContext);
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                activity?.SetExceptionTags(ex);
                throw;
            }
        }
    }

    public static IServiceCollection AddMediatRDefault(this IServiceCollection services)
    {
        // 註冊所有 MediatR 相關的服務
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TelemetryPipelineBehavior<,>));
        services.AddSingleton<MediatRTelemetry>();
        return services;
    }
}
