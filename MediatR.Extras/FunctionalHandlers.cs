using System;

namespace MediatR.Extras
{
    public class NotificationsDelegateWrapper<TNotification> : INotificationHandler<TNotification> where TNotification : INotification
    {
        private readonly Action<TNotification> action;

        public NotificationsDelegateWrapper(Action<TNotification> action)
        {
            this.action = action;
        }

        public void Handle(TNotification notification)
        {
            this.action(notification);
        }

        public override string ToString()
        {
            return GetType().CSharpName();
        }
    }

    public class RequestDelegateWrapper<TRequest> : IRequestHandler<TRequest, Unit> where TRequest : IRequest<Unit>
    {
        private readonly Action<TRequest> action;

        public RequestDelegateWrapper(Action<TRequest> action)
        {
            this.action = action;
        }

        public Unit Handle(TRequest message)
        {
            this.action(message);
            return new Unit();
        }

        public override string ToString()
        {
            return GetType().CSharpName();
        }
    }

    public class RequestDelegateWrapper<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly Func<TRequest, TResponse> requestDelegate;

        public RequestDelegateWrapper(Func<TRequest, TResponse> requestDelegate)
        {
            this.requestDelegate = requestDelegate;
        }

        public TResponse Handle(TRequest message)
        {
            return this.requestDelegate(message);
        }

        public override string ToString()
        {
            return GetType().CSharpName();
        }
    }
}