using System;
using MediatR;

namespace HangfireWithMediator.Abstractions;

public interface IJobRequest : IRequest
{
    public string JobId { get; set; }
    public string Cron { get; set; }
}
