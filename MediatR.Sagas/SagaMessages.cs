using System;
using MediatR.Extras;
using Newtonsoft.Json;

namespace MediatR.Sagas
{
    public class Load<TSagaState> : Request<int, SagaData<TSagaState>>, ICorrelated
        where TSagaState : class, ISagaState, new()
    {
        public DateTime When { get { return DateTime.Now; } }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(new
            {
                Id = Content,
                Type = typeof(TSagaState).CSharpName()
            });
        }

        public string CorrelationId
        {
            get { return Convert.ToString(Content); }
        }
    }

    public class SagaOver<TNotification> : Notification<TNotification>, ICorrelated
        where TNotification : INotification
    {
        public string CorrelationId
        {
            get
            {
                var correlated = Content as ICorrelated;
                return correlated == null ? null : correlated.CorrelationId;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(new
            {
                Type = typeof(TNotification).CSharpName(),
            });
        }
    }

    public class Upsert<TSagaState> : Command<SagaData<TSagaState>>, ICorrelated
        where TSagaState : class, ISagaState, new()
    {
        public string CorrelationId
        {
            get { return Convert.ToString(Content.Id); }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(new
            {
                Id = Content.Id,
                Type = typeof(TSagaState).CSharpName()
            });
        }
    }
}