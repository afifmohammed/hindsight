using Autofac;
using MediatR.Extras;

namespace MediatR.Hangfire
{
    public class CanInvokeHandlersUsingHangfire : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterGeneric(typeof(EnqueueHangfireJobForCommandHandler<,>))
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            builder.RegisterGeneric(typeof(EnqueueHangfireJobForEventHandler<,>))
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            builder
                .RegisterGeneric(typeof(ScheduleHangfireJobForCommandHandler<,>))
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            builder
                .RegisterGeneric(typeof(Scoped<,,>))
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder
                .RegisterGeneric(typeof(Scoped<,>))
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

        }
    }
}