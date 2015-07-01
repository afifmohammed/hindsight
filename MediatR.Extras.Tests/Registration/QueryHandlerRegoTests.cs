using System;
using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToAnswerQueriesCanResolveHandler
    {
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
        public void WhenRegisteredAsHandler()
        {
            ShouldFindHandler(builder => builder.RegisterQueryHandler<TimezoneHandler, Timezone, string>());
        }

        [Fact]
        public void WhenRegisteredAsHandlerBuilder()
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