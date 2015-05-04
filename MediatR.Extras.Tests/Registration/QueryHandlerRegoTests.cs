using System;
using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToAnswerQueriesCanResolveHandler
    {
        class Timezone : IRequest<string> { }

        class TimezoneProvider
        {
            public string Find(Timezone g)
            {
                return "AU";
            }
        }

        /// <remarks>
        /// this is a contrived example. 
        /// an abstraction over another abstraction is pointless.
        /// please do not use it as an example on how to design handlers. 
        /// </remarks>
        class TimezoneHandler : IRequestHandler<Timezone, string>
        {
            private readonly TimezoneProvider provider;

            public TimezoneHandler() : this(new TimezoneProvider())
            {}

            public TimezoneHandler(TimezoneProvider provider)
            {
                this.provider = provider;
            }

            public string Handle(Timezone message)
            {
                var timezone = this.provider.Find(message);
                return timezone;
            }
        }
        
        [Fact]
        public void WhenRegisteredAsADelegate()
        {
            ShouldFindHandler(builder => builder.RegisterQueryHandler<Timezone, string>(x => "AU"));
        }

        [Fact]
        public void WhenRegisteredAsADelegateBuilder()
        {
            ShouldFindHandler(builder => builder
                .With(x => x.RegisterType<TimezoneProvider>().AsSelf())
                .RegisterQueryHandler<Timezone, string>(c => c.Resolve<TimezoneProvider>().Find));
        }

        [Fact]
        public void WhenRegisteredAsType()
        {
            ShouldFindHandler(builder => builder.RegisterQueryHandler<TimezoneHandler, Timezone, string>());
        }

        [Fact]
        public void WhenRegisteredAsTypeBuilder()
        {
            ShouldFindHandler(builder => builder.With(x => x.RegisterType<TimezoneProvider>().AsSelf())
                .RegisterQueryHandler<TimezoneHandler, Timezone, string>(c =>
                {
                    var provider = c.Resolve<TimezoneProvider>();
                    return new TimezoneHandler(provider);
                }));
        }

        private static void ShouldFindHandler(Action<ContainerBuilder> builderSetup)
        {
            var builder = new ContainerBuilder();
            builderSetup(builder);

            var container = builder.Build();

            Assert.DoesNotThrow(() => container.Resolve<IRequestHandler<Timezone, string>>());
        }
    }
}