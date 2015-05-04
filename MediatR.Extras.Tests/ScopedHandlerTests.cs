using System;
using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToPublishEventsOnlyOnSuccessfulyCommits
    {
        class PolarIceMelts : INotification
        {
            public DateTime When { get; set; }
        }

        class ClimateChange : INotification
        {
            public DateTime When { get; set; }
        }

        public void ScopedHandlersPublishEventsAfterCommit()
        {
            var commit = DateTime.MinValue;
            var publish = DateTime.MinValue;
            
            new ContainerBuilder()
                /*
                .RegisterScopedEventHandler<PolarIceMelts>(c =>
                {
                    var mediator = c.Resolve<IMediator>();
                    return e =>
                    {
                        mediator.Publish(new ClimateChange());
                        commit = DateTime.Now;
                    };
                })
                */
                .RegisterEventHandler<ClimateChange>(e =>
                {
                    publish = DateTime.Now;
                })
                .Build()
                .Notify(new PolarIceMelts());
            
            Assert.True(publish > commit);
        }
    }
}