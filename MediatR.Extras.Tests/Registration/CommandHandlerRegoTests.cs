using System;
using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToSendRequestsCanResolveHandler
    {
        class OzyGreeting : IRequest { }

        class GreetingProvider
        {
            public void Greet(OzyGreeting g)
            { }
        }

        /// <remarks>
        /// this is a contrived example. 
        /// an abstraction over another abstraction is pointless.
        /// please do not use it as an example on how to design handlers. 
        /// </remarks>
        class GreetingHandler : IRequestHandler<OzyGreeting, Unit>
        {
            private readonly GreetingProvider provider;

            public GreetingHandler() : this(new GreetingProvider())
            {}

            public GreetingHandler(GreetingProvider provider)
            {
                this.provider = provider;
            }

            public Unit Handle(OzyGreeting message)
            {
                this.provider.Greet(message);
                return new Unit();
            }
        }
        
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
        public void WhenRegisteredAsType()
        {
            ShouldFindHandler(builder => builder.RegisterCommandHandler<GreetingHandler, OzyGreeting>());
        }

        [Fact]
        public void WhenRegisteredAsTypeBuilder()
        {
            ShouldFindHandler(builder => builder
                .With(x => x.RegisterType<GreetingProvider>().AsSelf())
                .RegisterCommandHandler<GreetingHandler, OzyGreeting>(c =>
                {
                    var provider = c.Resolve<GreetingProvider>();
                    return new GreetingHandler(provider);
                }));
        }

        private static void ShouldFindHandler(Action<ContainerBuilder> builderSetup)
        {
            var builder = new ContainerBuilder();
            builderSetup(builder);            
            var container = builder.Build();

            Assert.DoesNotThrow(() => container.Resolve<IRequestHandler<OzyGreeting, Unit>>());
        }
    }
}