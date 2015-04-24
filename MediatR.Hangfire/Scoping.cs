using Autofac;
using MediatR.Extras;

namespace MediatR.Hangfire
{
    class Scoped<THandler, TRequest, TReturn> : IRequestHandler<TRequest, TReturn> 
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
            {
                var mediator = requestScope.Resolve<THandler>();
                var result = mediator.Handle(message);
                requestScope.Resolve<Tasks>().ForEach(t => t());
                return result;
            }
        }
    }

    class Scoped<THandler, TNotification> : INotificationHandler<TNotification> 
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
            {
                var handler = requestScope.Resolve<THandler>();
                handler.Handle(notification);
                requestScope.Resolve<Tasks>().ForEach(t => t());
            }
        }
    }
}