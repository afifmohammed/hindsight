using System;
using System.Threading.Tasks;
using Hangfire;
using MediatR.Extras;
using MediatR.Sagas;

namespace MediatR.Hangfire
{
    class Schedule<THandler, TRequest, TReturn> : Request<TRequest, TReturn>
        where TRequest : IRequest<TReturn>
        where THandler : IRequestHandler<TRequest, TReturn>
    {
        public Type Handler { get { return typeof(THandler); } }
        public TimeSpan Interval { get; set; }
    }

    class ScheduleHangfireJobForCommandHandler<THandler, TRequest> : CanMediate, 
        IRequestHandler<Schedule<THandler, TRequest, Unit>, Unit> 
        where THandler : IRequestHandler<TRequest, Unit> 
        where TRequest : IRequest<Unit>
    {
        public Unit Handle(Schedule<THandler, TRequest, Unit> message)
        {
            var request = message.Content;
            BackgroundJob.Schedule<Scoped<THandler, TRequest, Unit>>(x => x.Handle(request), message.Interval);
            return new Unit();
        }
    }
}