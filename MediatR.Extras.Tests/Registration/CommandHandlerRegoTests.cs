using System;
using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToSendRequestsCanResolveHandler
    {
        [Fact]
        public void WhenRegisteredAsADelegate()
        {
            ShouldFindHandler(builder => builder.RegisterCommandHandler<OzyGreeting>(x => { }));
        }

        [Fact]
        public void WhenRegisteredAsADelegateBuilder()
        {
            ShouldFindHandler(builder => builder
                .With(x => x.RegisterType<GreetingProvider>().AsSelf())
                .RegisterCommandHandler<OzyGreeting>(c => c.Resolve<GreetingProvider>().Greet));
        }

        [Fact]
        public void WhenRegisteredAsHandler()
        {
            ShouldFindHandler(builder => builder.RegisterCommandHandler<GreetingHandler, OzyGreeting>());
        }

        [Fact]
        public void WhenRegisteredAsHandlerBuilder()
        {
            ShouldFindHandler(builder => builder
                .With(x => x.RegisterType<GreetingProvider>().AsSelf())
                .RegisterCommandHandler<GreetingHandler, OzyGreeting>(c =>
                {
                    var provider = c.Resolve<GreetingProvider>();
                    return new GreetingHandler(provider);
                }));
        }

        static void ShouldFindHandler(Action<ContainerBuilder> builderSetup)
        {
            var builder = new ContainerBuilder();
            builderSetup(builder);            
            var container = builder.Build();

            Assert.DoesNotThrow(() => container.Resolve<IRequestHandler<OzyGreeting, Unit>>());
        }
    }
}