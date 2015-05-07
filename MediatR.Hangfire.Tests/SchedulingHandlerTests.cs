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
        class ClimateChange : ITimeout, INotification
        {
            public TimeSpan Interval { get { return TimeSpan.FromSeconds(1); } }
        }

        private readonly List<Type> enqueudHandlers = new List<Type>();
        private readonly ILifetimeScope scope;

        public InOrderToDeliverTimeouts()
        {
            this.scope = new ContainerBuilder()
                .RegisterQueryHandler<Configured<EnqueueHandlers, bool>, bool>(c => true)
                .RegisterScheduledCommandHandler<RequestDelegateWrapper<Timeout<ClimateChange>>, Timeout<ClimateChange>>(
                    c => new RequestDelegateWrapper<Timeout<ClimateChange>>(r => {}))
                .RegisterCommandHandler<Schedule<RequestDelegateWrapper<Timeout<ClimateChange>>, Timeout<ClimateChange>, Unit>>(
                    r => enqueudHandlers.Add(r.Handler))
                .Build();
        }

        [Fact]
        public void ShouldReceiveRequestToScheduleTheHandler()
        {
            scope.Send(new Timeout<ClimateChange>());
            Assert.True(enqueudHandlers.Count(x => x == typeof(RequestDelegateWrapper<Timeout<ClimateChange>>)) == 1);
        }

        [Fact]
        public void ShouldRegisterTheHandlerAsScoped()
        {
            Assert.True(scope.Resolve<Scoped<RequestDelegateWrapper<Timeout<ClimateChange>>, Timeout<ClimateChange>, Unit>>() != null);
        }
    }
}