using System;
using System.Collections.Generic;
using Autofac;
using MediatR.Extras;
using MediatR.Sagas;
using Xunit;

namespace MediatR.Hangfire
{
    public class ClimateChange : INotification
    { }

    public class SchedulingHandlerTests
    {
        [Fact]
        public void CanRegisterTimeoutHandlers()
        {
            Assert.DoesNotThrow(() => new ContainerBuilder().With(x => x.RegisterModule<TimeoutHandlerModule>()).Build());
        }

        [Fact]
        public void ShouldReceiveScheduleHandlerRequest()
        {
            var list = new List<Schedule<ClimateChange>>();

            new ContainerBuilder()
                .RegisterQueryHandler<Configured<EnqueueHandlers, bool>, bool>(c => true)
                .With(x => x.RegisterModule<TimeoutHandlerModule>())
                .RegisterCommandHandler<Schedule<ClimateChange>>(list.Add)
                .Build()
                .Send(new Timeout<ClimateChange>
                {
                    Interval = TimeSpan.FromSeconds(1), 
                    Notification = new ClimateChange()
                });

            Assert.True(list.Count == 1);
        }
    }

}