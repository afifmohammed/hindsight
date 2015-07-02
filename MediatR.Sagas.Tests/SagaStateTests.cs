using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MediatR.Extras;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Sagas
{
    public class InOrderForSagasToHandleMessages
    {
        readonly ILifetimeScope scope;
        readonly IDictionary<int, SagaData<PaymentSagaState>> dictionary = new Dictionary<int, SagaData<PaymentSagaState>>();

        public InOrderForSagasToHandleMessages()
        {
            scope = new ContainerBuilder()
                .RegisterSaga<PaymentSaga, PaymentSagaState>()
                .RegisterCommandHandler<Upsert<PaymentSagaState>>(dictionary.Upsert)
                .RegisterQueryHandler<Load<PaymentSagaState>, SagaData<PaymentSagaState>>(dictionary.Load)
                .Build();
        }

        [Fact]
        public void ShouldUpdateStateWhenMessageIsHandled()
        {
            scope.Notify(new PaymentSubmitted {Order = 1});

            SagaData<PaymentSagaState> state;
            dictionary.TryGetValue(1, out state);

            Assert.NotNull(state);
            Assert.Equal(true, state.State.PaymentSubmitted);
        }

        [Fact]
        public void ShouldSatisfyInvariantsWhenMessagesAreHandled()
        {
            scope.Run(
                x => x.Publish(new PaymentSubmitted { Order = 1 }), 
                x => x.Publish(new StockReserved { Order = 1 })
                );

            SagaData<PaymentSagaState> state;
            dictionary.TryGetValue(1, out state);

            Assert.NotNull(state);
            var invariants = state.State.Invariants();
            
            Assert.Equal(true, invariants.All(i => i.Value()));
        }

        [Fact]
        public void ShouldUpdateStateWhenMarkedAsCompleted()
        {
            scope.Run(
                x => x.Publish(new PaymentSubmitted { Order = 1 }), 
                x => x.Publish(new StockReserved { Order = 1 })
                );

            SagaData<PaymentSagaState> state;
            dictionary.TryGetValue(1, out state);

            Assert.NotNull(state);
            
            Assert.Equal(true, state.MarkedAsComplete);
        }

        [Fact]
        public void ShouldNotSatisfyInvariantsWhenMessagesDoNotCorrelate()
        {
            scope.Run(
                x => x.Publish(new PaymentSubmitted { Order = 1 }), 
                x => x.Publish(new StockReserved { Order = 2 })
                );

            SagaData<PaymentSagaState> state;
            dictionary.TryGetValue(1, out state);

            Assert.NotNull(state);
            var invariants = state.State.Invariants();
            Assert.Equal(false, invariants[typeof(StockReserved).Name]());
        }
    }

    public class InOrderForSagasToMagicallyHandleMessages
    {
        readonly ILifetimeScope scope;
        readonly IDictionary<Int32, SagaData<PaymentSagaState>> dictionary = new Dictionary<Int32, SagaData<PaymentSagaState>>();

        public InOrderForSagasToMagicallyHandleMessages()
        {
            scope = new ContainerBuilder()
                .RegisterMagicSaga<MagicPaymentSaga, PaymentSagaState>()
                .RegisterCommandHandler<Upsert<PaymentSagaState>>(dictionary.Upsert)
                .RegisterQueryHandler<Load<PaymentSagaState>, SagaData<PaymentSagaState>>(dictionary.Load)
                .Build();
        }

        [Fact]
        public void ShouldUpdateStateWhenMessageIsHandled()
        {
            scope.Notify(new PaymentSubmitted { Order = 1 });

            SagaData<PaymentSagaState> state;
            dictionary.TryGetValue(1, out state);

            Assert.NotNull(state);
            Assert.Equal(true, state.State.PaymentSubmitted);
        }

        [Fact]
        public void ShouldSatisfyInvariantsWhenMessagesAreHandled()
        {
            scope.Run(
                x => x.Publish(new PaymentSubmitted { Order = 1 }),
                x => x.Publish(new StockReserved { Order = 1 })
                );

            SagaData<PaymentSagaState> state;
            dictionary.TryGetValue(1, out state);

            Assert.NotNull(state);
            var invariants = state.State.Invariants();

            Assert.Equal(true, invariants.All(i => i.Value()));
        }

        [Fact]
        public void ShouldUpdateStateWhenMarkedAsCompleted()
        {
            scope.Run(
                x => x.Publish(new PaymentSubmitted { Order = 1 }),
                x => x.Publish(new StockReserved { Order = 1 })
                );

            SagaData<PaymentSagaState> state;
            dictionary.TryGetValue(1, out state);

            Assert.NotNull(state);

            Assert.Equal(true, state.MarkedAsComplete);
        }

        [Fact]
        public void ShouldNotSatisfyInvariantsWhenMessagesDoNotCorrelate()
        {
            scope.Run(
                x => x.Publish(new PaymentSubmitted { Order = 1 }),
                x => x.Publish(new StockReserved { Order = 2 })
                );

            SagaData<PaymentSagaState> state;
            dictionary.TryGetValue(1, out state);

            Assert.NotNull(state);
            var invariants = state.State.Invariants();
            Assert.Equal(false, invariants[typeof(StockReserved).Name]());
        }
    }
}