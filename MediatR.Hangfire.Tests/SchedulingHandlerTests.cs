using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MediatR.Extras;
using MediatR.Extras.Tests;
using MediatR.Sagas;
using Xunit;

namespace MediatR.Hangfire
{
    public class InOrderToDeliverTimeouts
    {
        class ClimateChange : INotification
        {
            public DateTime When { get; set; }
        }

        [Fact]
        public void CanRegisterTimeoutHandlers()
        {
            Assert.DoesNotThrow(() => new ContainerBuilder().With(x => x.RegisterModule<TimeoutHandlerModule>()).Build());
        }

        [Fact]
        public void ShouldReceiveRequestToScheduleTheHandler()
        {
            var list = new List<Schedule<ClimateChange>>();

            new ContainerBuilder()
                .With(x => x.RegisterModule<TimeoutHandlerModule>())
                .RegisterQueryHandler<Configured<EnqueueHandlers, bool>, bool>(c => true)
                .RegisterCommandHandler<Schedule<ClimateChange>>(list.Add)
                .Build()
                .Send(new Timeout<ClimateChange>
                {
                    Notification = new ClimateChange { When = new DateTime(2050,1,1)}
                });

            Assert.Equal(1, list.Count(x => x.Notification.When == new DateTime(2050,1,1)));
        }
    }

}