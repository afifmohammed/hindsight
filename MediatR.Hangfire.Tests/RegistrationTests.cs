using Autofac;
using MediatR.Extras;
using Xunit;

namespace MediatR.Hangfire
{
    public class InOrderToCompletedEnqueueRequests
    {
        readonly ILifetimeScope scope;
        public InOrderToCompletedEnqueueRequests()
        {
            var container = new ContainerBuilder();
            container.RegisterModule<CanInvokeHandlersUsingHangfire>();
            scope = container.Build();
        }

        [Fact] 
        public void CanResolveTheHandlerThatSchedulesTheCommandHandler()
        {
            var handler = scope.Resolve<IRequestHandler<Schedule<BeepHandler, Beep, Unit>, Unit>>();
            Assert.True(handler is ScheduleHangfireJobForCommandHandler<BeepHandler, Beep>);
        }

        [Fact]
        public void CanResolveTheHandlerThatEnqueuesTheCommandHandler()
        {
            var handler = scope.Resolve<IRequestHandler<Enqueue<BeepHandler, Beep, Unit>, Unit>>();
            Assert.True(handler is EnqueueHangfireJobForCommandHandler<BeepHandler, Beep>);
        }

        [Fact]
        public void CanResolveTheHandlerThatEnqueuesTheNotificationHandler()
        {
            var handler = scope.Resolve<IRequestHandler<Enqueue<PingedHandler, Pinged>, Unit>>();
            Assert.True(handler is EnqueueHangfireJobForEventHandler<PingedHandler, Pinged>);
        }

        [Fact]
        public void CanResolveTheHandlerThatScopesTheCommandHandler()
        {
            Assert.DoesNotThrow(() => scope.Resolve<Scoped<BeepHandler, Beep, Unit>>());
        }

        [Fact]
        public void CanResolveTheHandlerThatScopesTheNotificationHandler()
        {
            Assert.DoesNotThrow(() => scope.Resolve<Scoped<PingedHandler, Pinged>>());
        }
    }
}
