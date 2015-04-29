using System;
using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToPublishEventsOnlyOnSuccessfulyCommits
    {
        public void ScopedHandlersPublishEventsAfterCommit()
        {
            var commit = DateTime.MinValue;
            var publish = DateTime.MinValue;
            
            new ContainerBuilder()
                .RegisterScopedEventHandler<PolarIceMelts>(c =>
                {
                    var mediator = c.Resolve<IMediator>();
                    return e =>
                    {
                        mediator.Publish(new ClimateChange());
                        commit = DateTime.Now;
                    };
                })
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