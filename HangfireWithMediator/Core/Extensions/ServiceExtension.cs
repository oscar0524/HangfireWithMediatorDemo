using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace HangfireWithMediator.Core.Extensions;

public static class ServiceDefaults
{
    public static IServiceCollection AddServiceDefault(this IServiceCollection services)
    {
        services.AddMediatR(config=>config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        return services;
    }
}
