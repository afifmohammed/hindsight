using System;
using MediatR.Extras;

namespace MediatR.Sagas
{
    public abstract class MagicSagaOf<TSagaState> : CanMediate
        where TSagaState : class, ISagaState, new()
    {
        #region Fields and Constructors

        private readonly HetrogenousContainer idMappers;

        protected MagicSagaOf()
        {
            idMappers = new HetrogenousContainer();

            ConfigureMessageMapping();
        }

        #endregion

        #region Public Members

        public SagaData<TSagaState> Saga { get; set; }

        public Int32 GetMessageIdFor<TMessage>(TMessage message)
        {
            Func<TMessage, Int32> map;

            if (!idMappers.TryGet(out map))
                throw MessageIdMapNotFoundException<TMessage>();

            return map(message);
        }

        public abstract Boolean TryComplete();

        #endregion

        #region Protected Members

        protected abstract void ConfigureMessageMapping();

        protected void Register<TMessage>(Func<TMessage, Int32> idMapper)
            where TMessage : class
        {
            idMappers.Put(idMapper);
        }

        protected void RequestTimeout<TNotification>(TNotification notification = null)
            where TNotification : class, ITimeout, INotification, new()
        {
            var message = notification ?? new TNotification();

            SendRequest(new Timeout<TNotification>
            {
                Notification = message
            });
        }

        #endregion

        #region Private Members

        private InvalidOperationException MessageIdMapNotFoundException<TMessage>()
        {
            const String messageFormat = "Message Id not mapped on {0} for {1}";

            var messageName = typeof(TMessage).CSharpName();
            var errorMessage = String.Format(messageFormat, this, messageName);

            throw new InvalidOperationException(errorMessage);
        }

        #endregion
    }
}