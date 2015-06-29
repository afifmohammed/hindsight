using System;
using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToDequeueEnqueuedHandlersCorrectly
    {
        class Beep : INotification { }
        int alerted;
        void ModifyState() { alerted = alerted + 1; }

        [Fact]
        public void ShouldStopDequeueingOnException()
        {
            var container = new ContainerBuilder()
                .RegisterEventHandler<EnqueueEventHandler<Beep>, Beep>(c => Handler<Beep>(ModifyState)(c.Resolve<Queue>()))
                .RegisterEventHandler<EnqueueEventHandler<Beep>, Beep>(c => Handler<Beep>(ModifyState, throws: true)(c.Resolve<Queue>()))
                .RegisterEventHandler<EnqueueEventHandler<Beep>, Beep>(c => Handler<Beep>(ModifyState, throws: true)(c.Resolve<Queue>()))
                .Build();

            Assert.Throws<InvalidOperationException>(() => container.Notify(new Beep()));
            Assert.True(alerted < 3);
        }

        [Fact]
        public void ShouldDequeueEveryHandler()
        {
            var container = new ContainerBuilder()
                .RegisterEventHandler<EnqueueEventHandler<Beep>, Beep>(c => Handler<Beep>(ModifyState)(c.Resolve<Queue>()))
                .RegisterEventHandler<EnqueueEventHandler<Beep>, Beep>(c => Handler<Beep>(ModifyState)(c.Resolve<Queue>()))
                .Build();

            Assert.DoesNotThrow(() => container.Notify(new Beep()));
            Assert.Equal(2, alerted);
        }

        static Func<Queue, EnqueueEventHandler<T>> Handler<T>(Action action, bool throws = false) where T : INotification
        {
            var handler = new NotificationsDelegateWrapper<T>(e =>
            {
                action();
                if (throws)
                    throw new InvalidOperationException();
            });
            return queue => new EnqueueEventHandler<T>(handler, queue);
        }
    }
}