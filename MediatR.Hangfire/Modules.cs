using Autofac;

namespace MediatR.Hangfire
{
    public class TimeoutHandlerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var mediator = c.Resolve<IMediator>();
                return new Channel(mediator.Publish);
            }).AsSelf();

            builder.RegisterGeneric(typeof (TimeoutRequestHandler<>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(ScheduledRequestHandler<>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();
        }
    }
}