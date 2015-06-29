using System;
using Autofac;
using MediatR.Extras;

namespace MediatR.Hangfire
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
                .Named<THandler>("inner");

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
                .Named<THandler>("inner");

            builder.RegisterEnqueuedInnerNotificationHandler<THandler, TNotification>();

            return builder;
        }

        static ContainerBuilder RegisterEnqueuedInnerNotificationHandler<THandler, TNotification>(this ContainerBuilder builder)
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
        {
            builder.RegisterType<Scoped<THandler, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder
                .Register(c =>
                {
                    var handler = c.ResolveNamed<THandler>("inner");
                    return new SendEnqueueRequestForEventHandler<THandler, TNotification>(handler);
                })
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<SendEnqueueRequestForEventHandler<THandler, TNotification>>("inner");

            builder.Register(c =>
            {
                var handler = c.ResolveNamed<SendEnqueueRequestForEventHandler<THandler, TNotification>>("inner");
                var queue = c.Resolve<Queue>();
                return new EnqueueEventHandler<TNotification>(handler, queue);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();

            builder.RegisterType<EnqueueHangfireJobForEventHandler<THandler, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }
    }
}