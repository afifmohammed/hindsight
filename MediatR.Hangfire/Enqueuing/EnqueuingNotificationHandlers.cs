using System;
using Hangfire;
using MediatR.Extras;

namespace MediatR.Hangfire
{
    class Enqueue<THandler, TNotification> : Request<TNotification, Unit>
        where THandler : INotificationHandler<TNotification>
        where TNotification : INotification
    {
        public Type Handler { get { return typeof(THandler); } }
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
}