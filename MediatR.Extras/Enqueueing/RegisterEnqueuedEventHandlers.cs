using System;
using Autofac;

namespace MediatR.Extras
{
    public static partial class Registrations
    {
        public static ContainerBuilder RegisterEnqueuedAuditor<TNotification>(this ContainerBuilder container) where TNotification : INotification, ICorrelated
        {
            container.RegisterEnqueuedEventHandler<AuditNotificationsHandler<TNotification>, TNotification>();
            return container;
        }

        public static ContainerBuilder RegisterEnqueuedEventHandler<THandler, TNotification>(this ContainerBuilder builder)
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
        {
            builder.RegisterType<THandler>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<THandler>("handler");

            builder.RegisterEnqueuedInnerNotificationHandler<THandler, TNotification>();

            return builder;
        }

        public static ContainerBuilder RegisterEnqueuedEventHandler<THandler, TNotification>(this ContainerBuilder builder,
            Func<IComponentContext, THandler> handlerBuilder)
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
        {
            builder.Register(handlerBuilder)
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<THandler>("handler");

            builder.RegisterEnqueuedInnerNotificationHandler<THandler, TNotification>();

            return builder;
        }

        static ContainerBuilder RegisterEnqueuedInnerNotificationHandler<THandler, TNotification>(this ContainerBuilder builder)
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
        {
            builder.Register(c =>
            {
                var queue = c.Resolve<Queue>();
                var handler = new SendEnqueueRequestForEventHandler<THandler, TNotification>(c.ResolveNamed<THandler>("handler"));
                c.InjectProperties(handler);
                return new EnqueueEventHandler<TNotification>(handler, queue);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();

            return builder;
        }
    }
}