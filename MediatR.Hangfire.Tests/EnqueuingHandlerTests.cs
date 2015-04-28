using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using MediatR.Extras;
using Xunit;

namespace MediatR.Hangfire
{
    public class InOrderToEnqueueHandlers
    {
        class PaymentReceived : INotification { }

        private readonly List<Type> enqueudHandlers = new List<Type>();
        private readonly ILifetimeScope scope;

        public InOrderToEnqueueHandlers()
        {
            this.scope = new ContainerBuilder()
                .RegisterQueryHandler<Configured<EnqueueHandlers, bool>, bool>(c => true)
                .RegisterEnqueuedEventHandler<NotificationsDelegateWrapper<PaymentReceived>, PaymentReceived>(
                    c => new NotificationsDelegateWrapper<PaymentReceived>(x => { }))
                .RegisterCommandHandler<Enqueue<NotificationsDelegateWrapper<PaymentReceived>, PaymentReceived>>(
                    r => enqueudHandlers.Add(r.Handler))
                .Build();
        }

        [Fact]
        public void ShouldReceiveRequestToEnqueueTheHandler()
        {
            scope.Notify(new PaymentReceived());
            Assert.True(enqueudHandlers.Count(x => x == typeof(NotificationsDelegateWrapper<PaymentReceived>)) == 1);
        }

        [Fact]
        public void ShouldRegisterTheHandlerAsScoped()
        {
            Assert.True(scope.Resolve<Scoped<NotificationsDelegateWrapper<PaymentReceived>, PaymentReceived>>() != null);
        }
    }

}
