using System;
using System.Collections.Generic;
using System.Linq;
using MediatR.Extras;
using MediatR.Sagas.Logging;

namespace MediatR.Sagas
{
    public sealed class MagicSagaNotificationHandler<TNotification, TSagaHandler, TSagaState> : CanMediate, INotificationHandler<TNotification>
        where TNotification : INotification
        where TSagaHandler : MagicSagaOf<TSagaState>, INotificationHandler<TNotification>
        where TSagaState : class, ISagaState, new()
    {
        #region Fields and Constructors

        private readonly ILog innerHandlerLogger = LogProvider.GetLogger(typeof(TSagaHandler).CSharpName());
        private readonly TSagaHandler sagaHandler;

        public MagicSagaNotificationHandler(TSagaHandler sagaHandler)
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

            var sagaExists = sagaHandler.Saga != null && sagaHandler.Saga.Id > 0;

            if (!sagaExists)
            {
                sagaHandler.Saga = new SagaData<TSagaState>
                {
                    Id = sagaId,
                    State = new TSagaState()
                };
            }

            if (sagaHandler.Saga.MarkedAsComplete)
            {
                // if the saga is complete, still call the handler method. the 
                // handler method on the saga handler should be robust enough
                // to realise it is complete, and handle this scenario accordingly
                ErrorLoggingHandle(notification)(sagaHandler);

                // ive removed this, because this message is already published
                // when the saga is marked as complete by the return of the TryComplete()
                //Publish(new SagaOver<TNotification> { Content = notification });

                return;
            }

            sagaHandler.Handle(notification);

            if (sagaHandler.Saga.IsPending == false)
            {
                if (sagaHandler.TryComplete())
                    Publish(new SagaOver<TNotification>
                    {
                        Content = notification
                    });
            }

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

            if (sagaExists)
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