using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MediatR.Extras;
using MediatR.Sagas;
using Xunit;

namespace MediatR.Hangfire
{
    public class InOrderToEnqueueSagaHandlers
    {
        private readonly List<Type> enqueudHandlers = new List<Type>();
        private readonly ILifetimeScope scope;
        
        public InOrderToEnqueueSagaHandlers()
        {
            this.scope = new ContainerBuilder()
                .RegisterQueryHandler<Configured<EnqueueHandlers, bool>, bool>(c => true)
                .RegisterEnqueuedSaga<PaymentSaga, PaymentSagaState>()
                .RegisterCommandHandler<Enqueue<SagaNotificationHandler<PaymentSubmitted, PaymentSaga, PaymentSagaState>, PaymentSubmitted>>(
                    r => enqueudHandlers.Add(r.Handler))
                .Build();
        }

        [Fact]
        public void ShouldReceiveRequestToEnqueueTheHandler()
        {
            scope.Notify(new PaymentSubmitted());
            Assert.True(enqueudHandlers.Count(x => x == typeof(SagaNotificationHandler<PaymentSubmitted, PaymentSaga, PaymentSagaState>)) == 1);
        }

        [Fact]
        public void ShouldRegisterTheHandlerAsScoped()
        {
            Assert.True(scope.Resolve<Scoped<SagaNotificationHandler<PaymentSubmitted, PaymentSaga, PaymentSagaState>, PaymentSubmitted>>() != null);
        }
    }
}