using System;
using System.Threading.Tasks;
using Hangfire;
using MediatR.Extras;
using MediatR.Sagas;

namespace MediatR.Hangfire
{
    class Schedule<TNotification> : IRequest where TNotification : INotification
    {
        public TNotification Notification { get; set; }
        public TimeSpan Interval { get; set; }
    }
    
    class TimeoutRequestHandler<TNotification> : CanMediate, IRequestHandler<Timeout<TNotification>, Unit> 
        where TNotification : class, INotification, new()
    {
        public Unit Handle(Timeout<TNotification> message)
        {
            var notification = message.Notification ?? new TNotification();

            if (SendRequest(new Configured<EnqueueHandlers, bool> {Default = true}))
                return SendRequest(new Schedule<TNotification>
                {
                    Interval = message.Interval,
                    Notification = message.Notification
                });

            Task.Delay(message.Interval);

            Publish(notification);

            return new Unit();
        }
    }

    class ScheduleHangfireJobForPublishing<TNotification> : CanMediate, IRequestHandler<Schedule<TNotification>, Unit>
            where TNotification : class, INotification, new()
    {
        public Unit Handle(Schedule<TNotification> message)
        {
            var notification = message.Notification ?? new TNotification();
            BackgroundJob.Schedule<Mediator>(x => x.Publish(notification), message.Interval);
            return new Unit();
        }
    }
}