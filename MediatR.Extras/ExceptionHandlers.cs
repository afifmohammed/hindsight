using System;
using System.Collections.Generic;
using System.Linq;
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
                this.log.LogError(new ExceptionLogging.Event {Exception = ex, Handler = this.handler, Input = notification});
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
                this.log.LogError(new ExceptionLogging.Event {Exception = ex, Handler = this.handler, Input = message});
                throw;
            }
        }

        public override string ToString()
        {
            return this.handler.ToString();
        }
    }

    public static class GlobalConfiguration
    {
        private static readonly IList<Type> ExceptionWhiteListing = new List<Type>();

        public static void MarkWhiteListed<TException>() where TException : Exception
        {
            if(ExceptionWhiteListing.All(t => t != typeof (TException)))
                ExceptionWhiteListing.Add(typeof(TException));
        }

        internal static LogLevel LogLevel(Exception exception)
        {
            var ignored = ExceptionWhiteListing.Any(t => t.IsInstanceOfType(exception));
            return ignored ? Logging.LogLevel.Info : Logging.LogLevel.Error;
        }

        public static bool IsWhiteListed(Exception exception)
        {
            return LogLevel(exception) != Logging.LogLevel.Error;
        }
    }

    static class ExceptionLogging
    {
        public struct Event
        {
            public object Input { get; set; }
            public object Handler { get; set; }
            public Exception Exception { get; set; }
        }

        public static void LogError(this ILog log, Event @event)
        {
            var correlated = @event.Input as ICorrelated;
            
            if (correlated != null)
                log.Log(GlobalConfiguration.LogLevel(@event.Exception), () => "{Handler} with Id {CorrelationId} on {Message} for {@Content} failed", @event.Exception,
                    @event.Handler,
                    correlated.CorrelationId,
                    correlated.GetType().CSharpName(),
                    correlated);

            if (correlated == null)
                log.Log(GlobalConfiguration.LogLevel(@event.Exception), () => "{Handler} on {Message} for {@Content} failed", @event.Exception,
                    @event.Handler,
                    @event.Input != null ? @event.Input.GetType().CSharpName() : null,
                    @event.Input);
        }
    }
}