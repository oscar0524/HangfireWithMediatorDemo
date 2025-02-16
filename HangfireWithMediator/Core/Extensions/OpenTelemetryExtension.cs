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

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

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
                        options.Filter = (httpContext) =>
                        {
                            if (ignorePath.Any(path => httpContext.Request.Path.StartsWithSegments(path)))
                            {
                                return false;
                            }
                            return true;
                        };
                    });
                tracing.AddOtlpExporter(otlpExporterOptions => { otlpExporterOptions.Endpoint = new Uri(otlpEndpoint); });
            })
            .WithLogging(logging =>
            {
                logging.AddOtlpExporter(otlpExporterOptions => { otlpExporterOptions.Endpoint = new Uri(otlpEndpoint); });
            });


        return builder;
    }
}
