using System;
using System.Collections.Generic;
using MediatR.Extras;

namespace MediatR.Sagas
{
    public abstract partial class SagaOf<TSagaState> : CanMediate
        where TSagaState : class, ISagaState, new()
    {
        #region Fields and Constructors

        private readonly IDictionary<Type, Func<Object, Int32>> messageIdMappers = new Dictionary<Type, Func<Object, Int32>>();
        private readonly IDictionary<Type, Func<TSagaState, Action<Object>>> messageContentMappers = new Dictionary<Type, Func<TSagaState, Action<Object>>>();

        protected SagaOf()
        {
            ConfigureMessageMapping();
        }

        #endregion

        #region Public Members

        public Int32 GetMessageIdFor<TMessage>(TMessage message)
        {
            Func<Object, Int32> map;

            if (!messageIdMappers.TryGetValue(typeof(TMessage), out map))
                throw MessageIdMapNotFoundException<TMessage>();

            return map(message);
        }

        public Boolean TryMapMessageContent<TMessage>(TMessage message)
        {
            Func<TSagaState, Action<Object>> mapper;

            if (messageContentMappers.TryGetValue(typeof(TMessage), out mapper))
            {
                mapper(Saga.State)(message);
                return true;
            }

            return false;
        }

        #endregion

        #region Protected Members

        protected abstract void ConfigureMessageMapping();

        protected void MapMessageId<TMessage>(Func<TMessage, Int32> getId) where TMessage : class
        {
            Func<Object, Int32> mapper = message => getId(message as TMessage);
            messageIdMappers[typeof(TMessage)] = mapper;
        }

        protected void MapMessage<TMessage>(Func<TMessage, Int32> getId,
            Func<TSagaState, Action<TMessage>> mapper) where TMessage : class
        {
            MapMessageId(getId);
            MapMessageContent(mapper);
        }

        #endregion

        #region Private Members

        private InvalidOperationException MessageIdMapNotFoundException<TMessage>()
        {
            const String messageFormat = "Message ID not mapped on {0} for {1}";

            var messageName = typeof(TMessage).CSharpName();
            var errorMessage = String.Format(messageFormat, this, messageName);

            throw new InvalidOperationException(errorMessage);
        }

        private void MapMessageContent<TMessage>(Func<TSagaState, Action<TMessage>> mapper) where TMessage : class
        {
            messageContentMappers.Add(typeof(TMessage), s => e => mapper(s)((TMessage)e));
        }

        #endregion
    }
}