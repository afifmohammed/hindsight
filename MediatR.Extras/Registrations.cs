using System;
using Autofac;

namespace MediatR.Extras
{
    public static class Registrations
    {
        public static ContainerBuilder RegisterQueryHandler<THandler, TRequest, TReturn>(this ContainerBuilder builder)
            where THandler : IRequestHandler<TRequest, TReturn> 
            where TRequest : IRequest<TReturn>
        {
            builder.RegisterType<THandler>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.Register(c =>
            {
                var handler = c.Resolve<THandler>();
                return new QueryHandler<TRequest, TReturn>(handler);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterQueryHandler<THandler, TRequest, TReturn>(this ContainerBuilder builder,
            Func<IComponentContext, THandler> handlerBuilder)
            where THandler : IRequestHandler<TRequest, TReturn>
            where TRequest : IRequest<TReturn>
        {
            builder.Register(handlerBuilder)
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.Register(c =>
            {
                var handler = c.Resolve<THandler>();
                return new QueryHandler<TRequest, TReturn>(handler);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterQueryHandler<TRequest, TReturn>(this ContainerBuilder builder, Func<TRequest, TReturn> handler)
            where TRequest : IRequest<TReturn>
        {
            builder.Register(c => new QueryHandler<TRequest, TReturn>(new RequestDelegateWrapper<TRequest, TReturn>(handler)))
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterQueryHandler<TRequest, TReturn>(this ContainerBuilder builder,
            Func<IComponentContext, Func<TRequest, TReturn>> handlerBuilder)
            where TRequest : IRequest<TReturn>
        {
            builder.Register(c =>
            {
                var handler = handlerBuilder(c);
                return new QueryHandler<TRequest, TReturn>(new RequestDelegateWrapper<TRequest, TReturn>(handler));
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterCommandHandler<TRequest>(this ContainerBuilder builder, Action<TRequest> handler)
            where TRequest : IRequest<Unit>
        {
            builder.Register(c => new RequestDelegateWrapper<TRequest>(handler))
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterCommandHandler<TRequest>(this ContainerBuilder builder, 
            Func<IComponentContext, Action<TRequest>> handlerBuilder)
            where TRequest : IRequest<Unit>
        {
            builder.Register(c =>
            {
                var handler = handlerBuilder(c);
                return new RequestDelegateWrapper<TRequest>(handler);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterCommandHandler<THandler, TRequest>(this ContainerBuilder builder)
            where TRequest : IRequest<Unit>
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder.RegisterType<THandler>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterCommandHandler<THandler, TRequest>(this ContainerBuilder builder,
            Func<IComponentContext, THandler> handlerBuilder)
            where TRequest : IRequest<Unit>
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder.Register(handlerBuilder)
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEventHandler<TNotification>(this ContainerBuilder builder, Action<TNotification> handler)
            where TNotification : INotification
        {
            builder.Register(c => new NotificationsDelegateWrapper<TNotification>(handler))
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterScopedEventHandler<TNotification>(this ContainerBuilder builder,
            Func<IComponentContext, Action<TNotification>> handlerBuilder)
            where TNotification : INotification
        {
            builder.Register(c =>
            {
                var handler = handlerBuilder(c);
                return new NotificationsDelegateWrapper<TNotification>(handler);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsSelf();

            builder.RegisterType<Scoped<NotificationsDelegateWrapper<TNotification>, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEventHandler<TNotification>(this ContainerBuilder builder, 
            Func<IComponentContext, Action<TNotification>> handlerBuilder)
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
                .AsImplementedInterfaces();
            
            return builder;
        }
    }
}