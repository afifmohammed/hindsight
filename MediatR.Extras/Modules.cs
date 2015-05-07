using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MediatR.Extras.Logging;

namespace MediatR.Extras
{
    public sealed class CanLookupConfiguredValues : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(LookupAppSettingsFor<,>)).As(typeof(IRequestHandler<,>));
            builder.RegisterGeneric(typeof(LookupAppSettingsFor<>)).As(typeof(IRequestHandler<,>));
            builder.RegisterGeneric(typeof(LookupConnectionStringFor<>)).As(typeof(IRequestHandler<,>));
        }
    }

    public sealed class CanUseAMediator : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();

                SingleInstanceFactory factory = context.Resolve;

                MultiInstanceFactory multiInstanceFactory =
                    x =>
                    {
                        var logger = new Lazy<ILog>(() => LogProvider.GetLogger(typeof(MultiInstanceFactory).CSharpName()));
                        var services = ((IEnumerable<object>) context.Resolve(typeof (IEnumerable<>).MakeGenericType(x))).ToList();
                        if (!services.Any())
                            logger.Value.Log(LogLevel.Warn, () => "0 Services found implementing {Contract}", null, x.CSharpName());
                        return services;
                    };

                return new Mediator(factory, multiInstanceFactory);
            })
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

            builder.RegisterType<Queue>()
                .AsSelf()
                .InstancePerLifetimeScope();
        }
    }
}