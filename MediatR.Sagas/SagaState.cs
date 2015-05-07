using System;
using System.Collections.Generic;
using System.Linq;
using MediatR.Extras;

namespace MediatR.Sagas
{
    public interface ISagaState
    {
        IDictionary<string, Func<bool>> Invariants();
    }

    public sealed class SagaData<TSagaState> 
        where TSagaState : class, ISagaState, new()
    {
        public int Id { get; set; }
        public bool MarkedAsComplete { get; set; }
        public TSagaState State { get; set; }
        public bool IsPending { get { return State.Invariants().Any(r => !r.Value()); } }
        public DateTime? LastModifiedUtc { get; set; }
    }

    public abstract partial class SagaOf<TSagaState> : CanMediate
        where TSagaState : class, ISagaState, new()
    {
        public SagaData<TSagaState> Saga { get; set; }
    }

    public static class InRequestStateManager
    {
        public static void Upsert<TSaga>(this IDictionary<int, SagaData<TSaga>> dictionary, Upsert<TSaga> command)
            where TSaga : class, ISagaState, new()
        {
            dictionary[command.Content.Id] = command.Content;
        }

        public static SagaData<TSaga> Load<TSaga>(this IDictionary<int, SagaData<TSaga>> dictionary, Load<TSaga> request) where TSaga : class, ISagaState, new()
        {
            SagaData<TSaga> saga;
            dictionary.TryGetValue(request.Content, out saga);
            return saga;
        }
    }
}