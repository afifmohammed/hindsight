using System;
using MediatR.Extras.Logging;

namespace MediatR.Extras
{
    public class ExceptionLoggingHandler<TNotification> : INotificationHandler<TNotification> where TNotification : INotification
    {
        private readonly INotificationHandler<TNotification> handler;
        private readonly ILog log;

        public ExceptionLoggingHandler(INotificationHandler<TNotification> handler)
        {
            this.handler = handler;
            log = LogProvider.GetLogger(handler.ToString());
        }

        public void Handle(TNotification notification)
        {
            try
            {
                this.handler.Handle(notification);
            }
            catch (Exception ex)
            {
                this.log.LogError(notification, ex);
                throw;
            }
        }

        public override string ToString()
        {
            return this.handler.ToString();
        }
    }

    public class ExceptionLoggingHandler<TRequest, TReturn> : IRequestHandler<TRequest, TReturn> where TRequest : IRequest<TReturn>
    {
        private readonly IRequestHandler<TRequest, TReturn> handler;
        private readonly ILog log;
        public ExceptionLoggingHandler(IRequestHandler<TRequest, TReturn> handler)
        {
            this.log = LogProvider.GetLogger(handler.ToString());
            this.handler = handler;
        }

        public TReturn Handle(TRequest message)
        {
            try
            {
                return this.handler.Handle(message);
            }
            catch (Exception ex)
            {
                this.log.LogError(message, ex);
                throw;
            }
        }

        public override string ToString()
        {
            return this.handler.ToString();
        }
    }

    static class ExceptionLogging
    {
        public static void LogError(this ILog log, object input, Exception exception)
        {
            var correlated = input as ICorrelated;
            if (correlated != null)
                log.Log(LogLevel.Error, () => "{Message} with {CorrelationId} for {@Content} failed", exception,
                    input != null ? input.GetType().CSharpName() : null,
                    input != null ? ((ICorrelated)input).CorrelationId : null,
                    input);

            if (correlated == null)
                log.Log(LogLevel.Error, () => "{Message} for {@Content} failed", exception,
                    input != null ? input.GetType().CSharpName() : null,
                    input);
        }
    }
}