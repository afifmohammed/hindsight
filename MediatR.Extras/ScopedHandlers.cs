using System;
using Autofac;

namespace MediatR.Extras
{
    public class Scoped<THandler, TRequest, TReturn> : IRequestHandler<TRequest, TReturn> 
        where TRequest : IRequest<TReturn>
        where THandler : IRequestHandler<TRequest, TReturn>
    {
        private readonly ILifetimeScope scope;

        public Scoped(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public TReturn Handle(TRequest message)
        {
            using (var requestScope = scope.BeginLifetimeScope())
            using (var events = requestScope.Resolve<Queue>())
            {
                var handler = requestScope.Resolve<THandler>();
                try
                {
                    var result = handler.Handle(message);
                    return result;
                }
                catch (Exception)
                {
                    events.Clear();
                    throw;
                }
            }
        }
    }

    public class Scoped<THandler, TNotification> : INotificationHandler<TNotification> 
        where TNotification : INotification
        where THandler : INotificationHandler<TNotification>
    {
        private readonly ILifetimeScope scope;

        public Scoped(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public void Handle(TNotification notification)
        {
            using (var requestScope = scope.BeginLifetimeScope())
            using (var events = requestScope.Resolve<Queue>())
            {
                var handler = requestScope.Resolve<THandler>();
                try
                {
                    handler.Handle(notification);
                }
                catch (Exception)
                {
                    events.Clear();
                    throw;
                }
            }
        }
    }
}