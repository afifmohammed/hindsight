using MediatR.Extras;

namespace MediatR.Hangfire
{
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
}