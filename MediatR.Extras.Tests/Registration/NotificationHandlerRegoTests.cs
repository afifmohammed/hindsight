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
        [Fact]
        public void WhenRegisteredAsHandler()
        {
            ShouldFindSubscribers(builder => builder
                .RegisterEventHandler<BlueToothHandler, BatteryRunningLow>()
                .RegisterEventHandler<WifiHandler, BatteryRunningLow>(),
                    new BlueToothHandler(), 
                    new WifiHandler());
        }

        [Fact]
        public void WhenRegisteredAsHandlerBuilder()
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

        private static void ShouldFindSubscribers(Action<ContainerBuilder> builderSetup, params INotificationHandler<BatteryRunningLow>[] expected)
        {
            var builder = new ContainerBuilder();
            builderSetup(builder);

            var subscribers = builder
                .Build()
                .Resolve<IEnumerable<INotificationHandler<BatteryRunningLow>>>()
                .ToList();

            Assert.Equal(expected.Count(), subscribers.Count());
            Assert.True(subscribers.All(s => expected.Any(i => i.ToString() == s.ToString())));
        }

    }
}