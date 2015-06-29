using System;
using System.Collections.Generic;
using System.Diagnostics;

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

    public class Queue : Queue<Action>, IDisposable
    {
        public void Dispose()
        {
            Action action = () => {};
            while (this.Count > 0)
                action += Dequeue();

            action();
        }
    }
}