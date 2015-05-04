using System;
using Autofac;
using MediatR.Extras.Logging;

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
            using (var queue = requestScope.Resolve<Queue>())
            {
                IRequestHandler<TRequest, TReturn> handler = requestScope.ResolveNamed<THandler>("inner");
                handler = handler is ExceptionLoggingHandler<TRequest, TReturn>
                    ? handler
                    : new ExceptionLoggingHandler<TRequest, TReturn>(handler);

                var logger = LogProvider.GetLogger(handler.ToString());
                try
                {
                    var result = handler.Handle(message);
                    return result;
                }
                catch (Exception ex)
                {
                    logger.LogError(message, ex);
                    queue.Clear();
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
            using (var queue = requestScope.Resolve<Queue>())
            {
                INotificationHandler<TNotification> handler = requestScope.ResolveNamed<THandler>("inner");
                handler = handler is ExceptionLoggingHandler<TNotification>
                    ? handler
                    : new ExceptionLoggingHandler<TNotification>(handler);
                try
                {
                    handler.Handle(notification);
                }
                catch (Exception)
                {
                    queue.Clear();
                    throw;
                }
            }
        }
    }
}