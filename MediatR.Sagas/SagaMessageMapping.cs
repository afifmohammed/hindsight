using System;
using System.Collections.Generic;

namespace MediatR.Sagas
{
    public abstract partial class SagaOf<TSagaState>
    {
        private readonly IDictionary<Type, Func<object, int>> messageIdMappers = new Dictionary<Type, Func<object, int>>();
        private readonly IDictionary<Type, Func<TSagaState, Action<object>>> messageContentMappers = new Dictionary<Type, Func<TSagaState, Action<object>>>();
        
        protected SagaOf()
        {
            ConfigureMessageMapping();
        }

        protected abstract void ConfigureMessageMapping();

        protected void MapMessageId<TMessage>(Func<TMessage, int> getId) where TMessage : class
        {
            Func<object, int> mapper = message => getId(message as TMessage);
            this.messageIdMappers[typeof(TMessage)] = mapper;
        }

        protected void MapMessage<TMessage>(Func<TMessage, int> getId, Func<TSagaState, Action<TMessage>> mapContent)
            where TMessage : class
        {
            MapMessageId(getId);
            MapMessageContent(mapContent);
        }

        public Func<object, int> MessageIdMap(Type messageType)
        {
            Func<object, int> map;

            if (!this.messageIdMappers.TryGetValue(messageType, out map))
                throw new InvalidOperationException(string.Format("Message ID not mapped on {0} for {1}", this, messageType.CSharpName()));

            return map;
        }

        public void MapMessageContent<TMessage>(TMessage message, out bool mapped)
        {
            Func<TSagaState, Action<object>> mapper;

            if (this.messageContentMappers.TryGetValue(typeof(TMessage), out mapper))
            {
                mapper(Saga.State)(message);
                mapped = true;
                return;
            }

            mapped = false;
        }

        private void MapMessageContent<TMessage>(Func<TSagaState, Action<TMessage>> mapper) where TMessage : class
        {
            this.messageContentMappers.Add(typeof(TMessage), s => e => mapper(s)((TMessage)e));
        }
    }
}