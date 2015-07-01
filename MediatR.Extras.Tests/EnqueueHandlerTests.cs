using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToCompleteEnqueueRequests
    {
        readonly ILifetimeScope scope;

        public InOrderToCompleteEnqueueRequests()
        {
            this.scope = new ContainerBuilder()
                .RegisterQueryHandler<Configured<EnqueueHandlers, bool>, bool>(c => true)
                .RegisterEnqueuedEventHandler<ManageBurnOuts, BurntOut>()
                .RegisterEnqueuedEventHandler<ManagerA, LayingLow>()
                .RegisterEnqueuedEventHandler<ManagerB, LayingLow>()
                .RegisterCommandHandler<Enqueue<ManageBurnOuts, BurntOut>>(c =>
                {
                    var handler = c.ResolveNamed<ManageBurnOuts>("handler");
                    return r => handler.Handle(r.Content);
                })
                .RegisterCommandHandler<Enqueue<ManagerA, LayingLow>>(c =>
                {
                    var handler = c.ResolveNamed<ManagerA>("handler");
                    return r => handler.Handle(r.Content);
                })
                .RegisterCommandHandler<Enqueue<ManagerB, LayingLow>>(c =>
                {
                    var handler = c.ResolveNamed<ManagerB>("handler");
                    return r => handler.Handle(r.Content);
                })
                .Build();
        }

        [Fact]
        public void ShouldInvokeAllEnqueuedHandlers()
        {
            Assert.DoesNotThrow(() => scope.Notify(new BurntOut()));
        }
    }
}