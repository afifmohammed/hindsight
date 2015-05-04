using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToNotifyCanResolveSubscribers
    {
        class BatteryRunningLow : INotification {}

        class WifiProvider { public void Toggle(bool off) { } }
        class BlueToothProvider { public void Toggle(bool off) { } }

        /// <remarks>
        /// this is a contrived example. 
        /// an abstraction over another abstraction is pointless.
        /// please do not use it as an example on how to design handlers. 
        /// </remarks>
        class WifiHandler : INotificationHandler<BatteryRunningLow>
        {
            private readonly WifiProvider provider;
            public WifiHandler() : this(new WifiProvider()) {}
            public WifiHandler(WifiProvider provider) { this.provider = provider; }

            public void Handle(BatteryRunningLow notification)
            {
                this.provider.Toggle(off:true);
            }

            public override string ToString()
            {
                return GetType().CSharpName();
            }
        }

        /// <remarks>
        /// this is a contrived example. 
        /// an abstraction over another abstraction is pointless.
        /// please do not use it as an example on how to design handlers. 
        /// </remarks>
        class BlueToothHandler : INotificationHandler<BatteryRunningLow>
        {
            private readonly BlueToothProvider provider;
            public BlueToothHandler() : this(new BlueToothProvider()) { }
            public BlueToothHandler(BlueToothProvider provider) { this.provider = provider; }

            public void Handle(BatteryRunningLow notification)
            {
                this.provider.Toggle(off: true);
            }

            public override string ToString()
            {
                return GetType().CSharpName();
            }
        }

        [Fact]
        public void WhenRegisteredAsType()
        {
            ShouldFindSubscribers(builder => builder
                .RegisterEventHandler<BlueToothHandler, BatteryRunningLow>()
                .RegisterEventHandler<WifiHandler, BatteryRunningLow>(),
                    new BlueToothHandler(), 
                    new WifiHandler());
        }

        [Fact]
        public void WhenRegisteredAsTypeBuilder()
        {
            ShouldFindSubscribers(builder => builder
                .With(x => x.RegisterType<BlueToothProvider>().AsSelf())
                .With(x => x.RegisterType<WifiProvider>().AsSelf())
                .RegisterEventHandler<BlueToothHandler, BatteryRunningLow>(c => new BlueToothHandler(c.Resolve<BlueToothProvider>()))
                .RegisterEventHandler<WifiHandler, BatteryRunningLow>(c => new WifiHandler(c.Resolve<WifiProvider>())),
                    new BlueToothHandler(), 
                    new WifiHandler());
        }

        [Fact]
        public void WhenRegisteredAsDelegate()
        {
            ShouldFindSubscribers(builder => builder
                .RegisterEventHandler<BatteryRunningLow>(e => {}),
                    new NotificationsDelegateWrapper<BatteryRunningLow>(e => {}));
        }

        [Fact]
        public void WhenRegisteredAsDelegateBuilder()
        {
            ShouldFindSubscribers(builder => builder
                .RegisterEventHandler<BatteryRunningLow>(c => e => c.Resolve<BlueToothProvider>().Toggle(true))
                .RegisterEventHandler<BatteryRunningLow>(c => e => c.Resolve<WifiProvider>().Toggle(true)),
                    new NotificationsDelegateWrapper<BatteryRunningLow>(e => { }),
                    new NotificationsDelegateWrapper<BatteryRunningLow>(e => { }));
        }

        private static void ShouldFindSubscribers(Action<ContainerBuilder> builderSetup, params INotificationHandler<BatteryRunningLow>[] instances)
        {
            var builder = new ContainerBuilder();
            builderSetup(builder);

            var subscribers = builder
                .Build()
                .Resolve<IEnumerable<INotificationHandler<BatteryRunningLow>>>()
                .ToList();

            Assert.Equal(instances.Count(), subscribers.Count());
            Assert.True(subscribers.All(s => instances.Any(i => i.ToString() == s.ToString())));
        }

    }
}