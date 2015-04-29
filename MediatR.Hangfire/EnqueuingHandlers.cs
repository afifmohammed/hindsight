using System;
using System.Collections.Generic;
using Hangfire;
using MediatR.Extras;

namespace MediatR.Hangfire
{
    class Enqueue<THandler, TNotification> : Request<TNotification, Unit>
        where THandler : INotificationHandler<TNotification>
        where TNotification : INotification
    {
        public Type Handler { get { return typeof (THandler); } }
    }

    class EnqueueHangfireJobForEventHandler<THandler, TNotification> : CanMediate, IRequestHandler<Enqueue<THandler, TNotification>, Unit>
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

    class SendEnqueueRequestForEventHandler<THandler, TNotification> : CanMediate, 
        INotificationHandler<TNotification>
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
    {
        private readonly THandler handler;
        
        public SendEnqueueRequestForEventHandler(THandler handler)
        {
            this.handler = handler;
        }

        public void Handle(TNotification notification)
        {
            if (!SendRequest(new Configured<EnqueueHandlers, bool> {Default = true} ))
            {
                this.handler.Handle(notification);
                return;
            }

            SendRequest(new Enqueue<THandler, TNotification> { Content = notification });
        }
    }

    class Enqueue<THandler, TRequest, TReturn> : Request<TRequest, TReturn>
        where TRequest : IRequest<TReturn>
        where THandler : IRequestHandler<TRequest, TReturn>
    {
        public Type Handler { get { return typeof(THandler); } }
    }

    class SendEnqueueRequestForCommandHandler<THandler, TRequest> : CanMediate, 
        IRequestHandler<TRequest, Unit>
            where TRequest : IRequest<Unit>
            where THandler : IRequestHandler<TRequest, Unit>
    {
        private readonly THandler handler;
        
        public SendEnqueueRequestForCommandHandler(THandler handler)
        {
            this.handler = handler;
        }

        public Unit Handle(TRequest request)
        {
            var enqueue = SendRequest(new Configured<EnqueueHandlers, bool> {Default = true});
            
            return enqueue
                ? SendRequest(new Enqueue<THandler, TRequest, Unit> {Content = request})
                : this.handler.Handle(request);
        }
    }

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

}