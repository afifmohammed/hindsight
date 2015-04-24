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
}