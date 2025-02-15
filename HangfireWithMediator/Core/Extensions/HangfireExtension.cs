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
    /// <summary>
    /// 資料庫路徑
    /// </summary>
    private const string DB_PATH = "Hangfire.db";

    /// <summary>
    /// 資料庫連線字串
    /// </summary>
    private const  string DB_CONNECTION_STRING= $"Data Source={DB_PATH}";

    /// <summary>
    /// 加入 Hangfire 預設設定
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddHangfireDefault(this IServiceCollection services)
    {
        // 建立Demo使用的 SQLite 資料庫
        services.AddDbContext<HangfireJobContext>(option=>option.UseSqlite(DB_CONNECTION_STRING));

        // 取得所有實作 IJobRequest 的類別並註冊
        var jobRequestType = typeof(IJobRequest);
        var jobTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => jobRequestType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
        foreach (var jobType in jobTypes)
        {
            services.AddSingleton(typeof(IJobRequest), jobType);
        }

        // 註冊用來啟用 HangfireJob 的服務
        services.AddSingleton<HangfireJobScheduler>();

        // 設定 Hangfire 並使用 SQLite 儲存資料
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage(DB_PATH); // 使用 SQLite 儲存資料
        });

        // 註冊 Hangfire 服務
        services.AddHangfireServer();

        return services;
    }

    /// <summary>
    /// 使用 Hangfire 啟動預設設定
    /// </summary>
    /// <param name="app"></param>
    public static void UseHangfireDefault(this IApplicationBuilder app)
    {
        // 測試環境下，每次啟動都清空資料庫
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HangfireJobContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        // 使用 Hangfire Dashboard 介面
        app.UseHangfireDashboard();

        // 啟動 Hangfire Job
        app.ApplicationServices.GetRequiredService<HangfireJobScheduler>().Start();
    }

}

/// <summary>
/// 執行Hangfire Job 的服務
/// </summary>
/// <param name="serviceProvider"></param>
public class HangfireJobScheduler(IServiceProvider serviceProvider)
{
    /// <summary>
    /// App執行時啟動用於註冊所有的Job
    /// </summary>
    public void Start()
    {
        var jobRequestList = serviceProvider.GetServices<IJobRequest>();
        foreach (var jobRequest in jobRequestList)
        {
            AddSchedule(jobRequest);
        }
    }

    /// <summary>
    /// 新增排程
    /// </summary>
    /// <param name="jobRequest"></param>
    public void AddSchedule(IJobRequest jobRequest)
    {
        // 移除舊的排程，可以避免過了執行時間的排程自動啟動
        RecurringJob.RemoveIfExists(jobRequest.JobId);
        RecurringJob.AddOrUpdate(jobRequest.JobId, () => RunJob(jobRequest), jobRequest.Cron, new RecurringJobOptions(){
            TimeZone = TimeZoneInfo.Local // 使用本機時區
        });
    }

    /// <summary>
    /// 執行Job
    /// </summary>
    /// <param name="jobRequest"></param>
    public void RunJob(IJobRequest jobRequest)
    {
        // 因為執行這個環境是在Singleton的環境下，所以需要重新建立Scope
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        mediator.Send(jobRequest);
    }

}
