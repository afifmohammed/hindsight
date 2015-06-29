using System;
using System.Linq;
using MediatR.Extras;
using MediatR.Sagas.Logging;

namespace MediatR.Sagas
{
    public sealed class SagaNotificationHandler<TNotification, TSagaHandler, TSagaState> : CanMediate, INotificationHandler<TNotification>
        where TNotification : INotification
        where TSagaHandler : SagaOf<TSagaState>, INotificationHandler<TNotification>
        where TSagaState : class, ISagaState, new()
    {
        private readonly TSagaHandler innerHandler;
        private readonly ILog innerHandlerLogger = LogProvider.GetLogger(typeof(TSagaHandler).CSharpName());

        public SagaNotificationHandler(TSagaHandler innerHandler)
        {
            this.innerHandler = innerHandler;
        }

        public void Handle(TNotification notification)
        {
            var id = this.innerHandler.MessageIdMap(notification.GetType())(notification);
            this.innerHandler.Saga = SendRequest(new Load<TSagaState> { Content = id });

            var exists = this.innerHandler.Saga != null && this.innerHandler.Saga.Id > 0;

            if (!exists)
            {
                this.innerHandler.Saga = new SagaData<TSagaState>
                {
                    Id = id,
                    MarkedAsComplete = false,
                    State = new TSagaState()
                };
            }

            if (this.innerHandler.Saga.MarkedAsComplete)
            {
                ErrorLoggingHandle(notification)(this.innerHandler);
                Publish(new SagaOver<TNotification> { Content = notification });
                return;
            }

            bool mapped;
            this.innerHandler.MapMessageContent(notification, out mapped);
            if ((mapped && this.innerHandler.Saga.IsPending) == false)
                ErrorLoggingHandle(notification)(this.innerHandler);

            var voilation = this.innerHandler.Saga.State.Invariants().FirstOrDefault(invariant => !invariant.Value());
            if (!string.IsNullOrEmpty(voilation.Key))
                this.innerHandlerLogger.Log(LogLevel.Info, () => "{Handler} with Id {CorrelationId} on {Message} is pending {Invariant}", null,
                    this.innerHandler,
                    this.innerHandler.Saga.Id,
                    notification.GetType().CSharpName(),
                    voilation.Key);

            if (this.innerHandler.Saga.MarkedAsComplete)
                this.innerHandlerLogger.Log(LogLevel.Info, () => "{Handler} with Id {CorrelationId} on {Message} is marked as complete", null,
                    this.innerHandler,
                    this.innerHandler.Saga.Id,
                    notification.GetType().CSharpName());

            SendCommand(new Upsert<TSagaState> { Content = this.innerHandler.Saga });

            if (exists)
                this.innerHandlerLogger.Log(LogLevel.Info, () => "Updated {Handler} with Id {CorrelationId} and Version {RowVersion} when handling {Message}", null,
                    this.innerHandler.ToString(),
                    this.innerHandler.Saga.Id,
                    BitConverter.ToString(this.innerHandler.Saga.CurrentVersion),
                    notification.GetType().CSharpName());
        }

        private static Action<INotificationHandler<TNotification>> ErrorLoggingHandle(TNotification notification)
        {
            return inner =>
            {
                var handler = inner is ExceptionLoggingHandler<TNotification>
                ? inner
                : new ExceptionLoggingHandler<TNotification>(inner);

                handler.Handle(notification);
            };
        }

        public override string ToString()
        {
            return this.innerHandler.ToString();
        }
    }
}