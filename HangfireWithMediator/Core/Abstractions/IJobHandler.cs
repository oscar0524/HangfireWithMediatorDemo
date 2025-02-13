using System;
using MediatR;

namespace HangfireWithMediator.Abstractions;

public interface IJobHandler<T>:IRequestHandler<T> where T : IJobRequest
{

}
