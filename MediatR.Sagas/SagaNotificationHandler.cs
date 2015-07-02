using System;
using System.Collections.Generic;
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
        #region Fields and Constructors

        private readonly ILog innerHandlerLogger = LogProvider.GetLogger(typeof(TSagaHandler).CSharpName());
        private readonly TSagaHandler sagaHandler;

        public SagaNotificationHandler(TSagaHandler sagaHandler)
        {
            this.sagaHandler = sagaHandler;
        }

        #endregion

        #region Interface Implementations

        public void Handle(TNotification notification)
        {
            var sagaId = sagaHandler.GetMessageIdFor(notification);

            sagaHandler.Saga = SendRequest(new Load<TSagaState>
            {
                Content = sagaId
            });

            var exists = sagaHandler.Saga != null && sagaHandler.Saga.Id > 0;

            if (!exists)
            {
                sagaHandler.Saga = new SagaData<TSagaState>
                {
                    Id = sagaId,
                    State = new TSagaState()
                };
            }
            else
            {
                if (sagaHandler.Saga.MarkedAsComplete)
                {
                    ErrorLoggingHandle(notification)(sagaHandler);

                    Publish(new SagaOver<TNotification>
                    {
                        Content = notification
                    });

                    return;
                }
            }

            var mapped = sagaHandler.TryMapMessageContent(notification);

            if ((mapped && sagaHandler.Saga.IsPending) == false)
                ErrorLoggingHandle(notification)(sagaHandler);

            var violation = sagaHandler.Saga.State.Invariants()
                .FirstOrDefault(invariant => !invariant.Value());

            if (!String.IsNullOrEmpty(violation.Key))
                LogSagaViolation(violation);

            if (sagaHandler.Saga.MarkedAsComplete)
                LogSagaMarkedAsComplete();

            SendCommand(new Upsert<TSagaState>
            {
                Content = sagaHandler.Saga
            });

            if (exists)
                LogSagaUpdate();
        }

        #endregion

        #region Overrides

        public override String ToString()
        {
            return sagaHandler.ToString();
        }

        #endregion

        #region Private Members

        private void LogSagaViolation(KeyValuePair<String, Func<Boolean>> voilation)
        {
            innerHandlerLogger.Log(LogLevel.Info, () => "{Handler} with Id {CorrelationId} on {Message} is pending {Invariant}", null,
                sagaHandler,
                sagaHandler.Saga.Id,
                typeof(TNotification).CSharpName(),
                voilation.Key);
        }

        private void LogSagaMarkedAsComplete()
        {
            innerHandlerLogger.Log(LogLevel.Info, () => "{Handler} with Id {CorrelationId} on {Message} is marked as complete", null,
                sagaHandler,
                sagaHandler.Saga.Id,
                typeof(TNotification).CSharpName());
        }

        private void LogSagaUpdate()
        {
            innerHandlerLogger.Log(LogLevel.Info, () => "Updated {Handler} with Id {CorrelationId} and Version {RowVersion} when handling {Message}", null,
                sagaHandler.ToString(),
                sagaHandler.Saga.Id,
                BitConverter.ToString(sagaHandler.Saga.CurrentVersion),
                typeof(TNotification).CSharpName());
        }

        private static Action<INotificationHandler<TNotification>> ErrorLoggingHandle(TNotification notification)
        {
            return inner =>
            {
                var handler = inner as ExceptionLoggingHandler<TNotification>
                    ?? new ExceptionLoggingHandler<TNotification>(inner);

                handler.Handle(notification);
            };
        }

        #endregion
    }
}