using System;
using System.Collections.Generic;

namespace MediatR.Extras
{
    public class EnqueueRequestHandler<TRequest> : IRequestHandler<TRequest, Unit>
        where TRequest : IRequest<Unit>
    {
        private readonly IRequestHandler<TRequest, Unit> handler;
        private readonly Queue<Action> queue;

        public EnqueueRequestHandler(IRequestHandler<TRequest, Unit> handler, Queue<Action> queue)
        {
            this.handler = handler is ExceptionLoggingHandler<TRequest, Unit> 
                ? handler 
                : new ExceptionLoggingHandler<TRequest, Unit>(handler);

            this.queue = queue;
        }

        public Unit Handle(TRequest message)
        {
            this.queue.Enqueue(() => this.handler.Handle(message));
            return new Unit();
        }
    }

    public class EnqueueEventHandler<TNotification> : INotificationHandler<TNotification> where TNotification : INotification
    {
        private readonly INotificationHandler<TNotification> handler;
        private readonly Queue<Action> queue;

        public EnqueueEventHandler(INotificationHandler<TNotification> handler, Queue<Action> queue)
        {
            this.handler = handler is ExceptionLoggingHandler<TNotification>
                ? handler
                : new ExceptionLoggingHandler<TNotification>(handler);

            this.queue = queue;
        }

        public void Handle(TNotification notification)
        {
            this.queue.Enqueue(() => handler.Handle(notification));
        }
    }

    public struct EnqueueHandlers
    { }

    public class Enqueue<THandler, TNotification> : Request<TNotification, Unit>
        where THandler : INotificationHandler<TNotification>
        where TNotification : INotification
    {
        public Type Handler { get { return typeof(THandler); } }
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
            if (!SendRequest(new Configured<EnqueueHandlers, bool> { Default = true }))
            {
                this.handler.Handle(notification);
                return;
            }

            SendRequest(new Enqueue<THandler, TNotification> { Content = notification });
        }
    }

    public class Enqueue<THandler, TRequest, TReturn> : Request<TRequest, TReturn>
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
            var enqueue = SendRequest(new Configured<EnqueueHandlers, bool> { Default = true });

            return enqueue
                ? SendRequest(new Enqueue<THandler, TRequest, Unit> { Content = request })
                : this.handler.Handle(request);
        }
    }

    public class Queue : Queue<Action>, IDisposable
    {
        public void Dispose()
        {
            while (this.Count > 0)
                Dequeue()();
        }
    }
}