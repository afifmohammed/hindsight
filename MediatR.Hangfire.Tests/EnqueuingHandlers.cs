using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using MediatR.Extras;
using Xunit;

namespace MediatR.Hangfire
{
    class ItsNewYears : INotification { }
    
    public class EnqueueingEventHandlerTests
    {
        private readonly List<Type> enqueudHandlers = new List<Type>();
        private readonly Func<ILifetimeScope> scope;

        public EnqueueingEventHandlerTests()
        {
            this.scope = () => new ContainerBuilder()
                .RegisterQueryHandler<Configured<EnqueueHandlers, bool>, bool>(c => true)
                .RegisterEnqueuedEventHandler<NotificationsDelegateWrapper<ItsNewYears>, ItsNewYears>(
                    c => new NotificationsDelegateWrapper<ItsNewYears>(x => { }))
                .RegisterCommandHandler<Enqueue<NotificationsDelegateWrapper<ItsNewYears>, ItsNewYears>>(
                    r => enqueudHandlers.Add(r.Handler))
                .Build();
        }

        [Fact]
        public void ShouldReceiveEnqueueHandlerRequest()
        {
            scope().Notify(new ItsNewYears());
            Assert.True(enqueudHandlers.Count(x => x == typeof(NotificationsDelegateWrapper<ItsNewYears>)) == 1);
        }

        [Fact]
        public void CanResolveScopedHandler()
        {
            Assert.True(scope().Resolve<Scoped<NotificationsDelegateWrapper<ItsNewYears>, ItsNewYears>>() != null);
        }
    }

}
