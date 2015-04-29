using System.Configuration;
using Autofac;
using MediatR.Extras.Tests;
using Xunit;

namespace MediatR.Extras
{
    public class InOrderToLookupConfigurationValues
    {
        struct MyKey { }

        [Fact]
        public void CanRegisterConfigurationModule()
        {
            Assert.DoesNotThrow(() => new ContainerBuilder().RegisterModule<CanLookupConfiguredValues>());
        }

        [Fact]
        public void ShouldHandleRequestForConfiguredValue()
        {
            Assert.DoesNotThrow(() => 
                new ContainerBuilder()
                    .With(x => x.Register<AppSettingsProvider>(c => k => "value"))
                    .Build()
                    .Request(new Configured<MyKey, string>()));
        }

        [Fact]
        public void ShouldReturnConfiguredValue()
        {
            var value = new ContainerBuilder()
                .With(x => x.Register<AppSettingsProvider>(c => k => "100"))
                .Build()
                .Request(new Configured<MyKey, int>());

            Assert.Equal(100, value);
        }

        [Fact]
        public void ShouldReturnConfiguredValueIgnoringDefault()
        {
            var value = new ContainerBuilder()
                .With(x => x.Register<AppSettingsProvider>(c => k => "100"))
                .Build()
                .Request(new Configured<MyKey, int> {Default = 10});

            Assert.Equal(100, value);
        }

        [Fact]
        public void ShouldReturnConfiguredDefaultValueForMissingKey()
        {
            var value = new ContainerBuilder()
                .With(x => x.Register<AppSettingsProvider>(c => k => null))
                .Build()
                .Request(new Configured<MyKey, int> {Default = 10});

            Assert.Equal(10, value);
        }

        [Fact]
        public void ShouldThrowExceptionForMissingKeyWithNoDefault()
        {
            Assert.Throws<ConfigurationErrorsException>(() => 
                new ContainerBuilder()
                    .With(x => x.Register<AppSettingsProvider>(c => k => null))
                    .Build()
                    .Request(new Configured<MyKey, int>()));
        }
    }
}