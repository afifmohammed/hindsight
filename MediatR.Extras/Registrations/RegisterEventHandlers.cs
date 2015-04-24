using System;
using Autofac;

namespace MediatR.Extras
{
    public static partial class Registrations
    {
        public static ContainerBuilder RegisterEventHandler<TNotification>(this ContainerBuilder builder, Action<TNotification> handler)
            where TNotification : INotification
        {
            builder.Register(c => new NotificationsDelegateWrapper<TNotification>(handler))
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEventHandler<TNotification>(this ContainerBuilder builder, Func<IComponentContext, Action<TNotification>> handlerBuilder)
            where TNotification : INotification
        {
            builder.Register(c =>
            {
                var handler = handlerBuilder(c);
                return new NotificationsDelegateWrapper<TNotification>(handler);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEventHandler<THandler, TNotification>(this ContainerBuilder builder)
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
        {
            builder.RegisterType<THandler>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEventHandler<THandler, TNotification>(this ContainerBuilder builder,
            Func<IComponentContext, THandler> handlerBuilder)
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
        {
            builder.Register(handlerBuilder)
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.RegisterType<Scoped<THandler, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterPublisher<THandler, TRequest>(this ContainerBuilder builder)
            where TRequest : IRequest<Unit>
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder.RegisterType<THandler>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.RegisterType<Scoped<THandler, TRequest, Unit>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterPublisher<THandler, TRequest>(this ContainerBuilder builder,
            Func<IComponentContext, THandler> handlerBuilder)
            where TRequest : IRequest<Unit>
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder.Register(handlerBuilder)
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.RegisterType<Scoped<THandler, TRequest, Unit>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }
    }
}