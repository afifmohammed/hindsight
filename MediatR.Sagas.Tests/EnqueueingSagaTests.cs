using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MediatR.Extras;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Sagas
{
    public class InOrderToEnqueueSagaHandlers
    {
        readonly List<Type> enqueudHandlers = new List<Type>();
        
        public InOrderToEnqueueSagaHandlers()
        {
            new ContainerBuilder()
                .RegisterQueryHandler<Configured<EnqueueHandlers, bool>, bool>(c => true)
                .RegisterEnqueuedSaga<PaymentSaga, PaymentSagaState>()
                .RegisterCommandHandler<Enqueue<SagaNotificationHandler<PaymentSubmitted, PaymentSaga, PaymentSagaState>, PaymentSubmitted>>(
                    r => enqueudHandlers.Add(r.Handler))
                .Build()
                .Notify(new PaymentSubmitted());
        }

        [Fact]
        public void ShouldReceiveRequestToEnqueueTheHandler()
        {
            Assert.True(enqueudHandlers.Count(x => x == typeof(SagaNotificationHandler<PaymentSubmitted, PaymentSaga, PaymentSagaState>)) == 1);
        }
    }
}