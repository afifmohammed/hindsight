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
        private readonly ILog innerHandlerLogger = LogProvider.For<TSagaHandler>();

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
                Publish(new SagaOver<TNotification> { Content = notification });
                return;
            }

            bool mapped;
            this.innerHandler.MapMessageContent(notification, out mapped);
            if ((mapped && this.innerHandler.Saga.IsPending) == false)
                this.innerHandler.Handle(notification);

            var voilation = this.innerHandler.Saga.State.Invariants().FirstOrDefault(invariant => !invariant.Value());
            if (!string.IsNullOrEmpty(voilation.Key))
                this.innerHandlerLogger.Log(LogLevel.Info, () => "{Handler} with Id {CorrelationId} is pending on {Message}", null,
                    this.innerHandler.ToString(),
                    this.innerHandler.Saga.Id,
                    voilation.Key);

            if (this.innerHandler.Saga.MarkedAsComplete)
                this.innerHandlerLogger.Log(LogLevel.Info, () => "{Handler} with Id {CorrelationId} is marked as complete",
                    null,
                    this.innerHandler.ToString(),
                    this.innerHandler.Saga.Id);

            SendCommand(new Upsert<TSagaState> { Content = this.innerHandler.Saga });
        }

        public override string ToString()
        {
            return this.innerHandler.ToString();
        }
    }
}