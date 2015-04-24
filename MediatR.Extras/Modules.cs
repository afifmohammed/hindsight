using System.Collections.Generic;
using Autofac;

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

                MultiInstanceFactory multiInstanceFactory = x => (IEnumerable<object>) context.Resolve(typeof(IEnumerable<>).MakeGenericType(x));
                
                return new Mediator(factory, multiInstanceFactory);
            }).AsImplementedInterfaces();

            builder.RegisterType<Tasks>()
                .AsSelf()
                .InstancePerLifetimeScope();
        }
    }
}