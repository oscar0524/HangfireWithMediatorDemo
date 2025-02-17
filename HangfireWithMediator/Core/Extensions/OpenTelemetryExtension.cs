using System.Diagnostics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace HangfireWithMediator.Core.Extensions;

public static class OpenTelemetryExtension
{
    public static IHostApplicationBuilder AddOpenTelemetryDefault(this IHostApplicationBuilder builder)
    {
        var otlpEndpoint = builder.Configuration.GetValue("Otlp:Endpoint", defaultValue: "http://localhost:4317");
        var ignorePath = new string[] { "/hangfire" };

        builder.Logging.ClearProviders();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource((resource) => resource.AddService(serviceName: builder.Environment.ApplicationName))
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation();

                metrics.AddOtlpExporter(otlpExporterOptions => { otlpExporterOptions.Endpoint = new Uri(otlpEndpoint); });
            })
            .WithTracing(tracing =>
            {
                tracing.AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = (httpContext) => ignorePath.Any(path => httpContext.Request.Path.StartsWithSegments(path)) == false;
                    });
                tracing.AddOtlpExporter(otlpExporterOptions => { otlpExporterOptions.Endpoint = new Uri(otlpEndpoint); });
            })
            .WithLogging(logging =>
            {
                logging.AddOtlpExporter(otlpExporterOptions => { otlpExporterOptions.Endpoint = new Uri(otlpEndpoint); });
            });

        return builder;
    }

    // See https://opentelemetry.io/docs/specs/otel/trace/semantic_conventions/exceptions/
    public static void SetExceptionTags(this Activity activity, Exception ex)
    {
        if (activity is null)
        {
            return;
        }

        activity.AddTag("exception.message", ex.Message);
        activity.AddTag("exception.stacktrace", ex.ToString());
        activity.AddTag("exception.type", ex.GetType().FullName);
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
