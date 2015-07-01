using Hangfire;
using MediatR.Extras;

namespace MediatR.Hangfire
{
    class EnqueueHangfireJobForCommandHandler<THandler, TRequest> : CanMediate,
        IRequestHandler<Enqueue<THandler, TRequest, Unit>, Unit>
        where TRequest : IRequest<Unit>
        where THandler : IRequestHandler<TRequest, Unit>
    {
        public Unit Handle(Enqueue<THandler, TRequest, Unit> message)
        {
            var request = message.Content;
            BackgroundJob.Enqueue<Scoped<THandler, TRequest, Unit>>(x => x.Handle(request));
            return new Unit();
        }
    }

    class EnqueueHangfireJobForEventHandler<THandler, TNotification> : CanMediate,
        IRequestHandler<Enqueue<THandler, TNotification>, Unit>
        where TNotification : INotification
        where THandler : INotificationHandler<TNotification>
    {
        public Unit Handle(Enqueue<THandler, TNotification> message)
        {
            var notification = message.Content;
            BackgroundJob.Enqueue<Scoped<THandler, TNotification>>(x => x.Handle(notification));
            return new Unit();
        }
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