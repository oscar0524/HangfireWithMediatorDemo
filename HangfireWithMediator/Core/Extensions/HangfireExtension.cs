using System;
using System.Reflection;
using Hangfire;
using Hangfire.Storage.SQLite;
using HangfireWithMediator.Abstractions;
using HangfireWithMediator.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HangfireWithMediator.Core.Extensions;

public static class HangfireDefaults
{
    private const  string DB_CONNECTION_STRING= "Data Source=Hangfire.db";
    public static IServiceCollection AddHangfireDefault(this IServiceCollection services)
    {
        services.AddDbContext<HangfireJobContext>(option=>option.UseSqlite(DB_CONNECTION_STRING));

        var jobRequestType = typeof(IJobRequest);
        var jobTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => jobRequestType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var jobType in jobTypes)
        {
            services.AddSingleton(typeof(IJobRequest), jobType);
        }

        services.AddSingleton<HangfireJobScheduler>();

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage(DB_CONNECTION_STRING);
        });

        services.AddHangfireServer();

        return services;
    }

    public static void UseHangfireDefault(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HangfireJobContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        app.UseHangfireDashboard();
        app.ApplicationServices.GetRequiredService<HangfireJobScheduler>().Start();
    }

}

public class HangfireJobScheduler(IServiceProvider serviceProvider)
{
    public void Start()
    {
        var jobRequestList = serviceProvider.GetServices<IJobRequest>();
        foreach (var jobRequest in jobRequestList)
        {
            AddSchedule(jobRequest);
        }
    }

    public void AddSchedule(IJobRequest jobRequest)
    {
        RecurringJob.RemoveIfExists(jobRequest.JobId);
        RecurringJob.AddOrUpdate(jobRequest.JobId, () => RunJob(jobRequest), jobRequest.Cron);
    }

    public void RunJob(IJobRequest jobRequest)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        mediator.Send(jobRequest);
    }

}
