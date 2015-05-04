using System;
using Autofac;
using Autofac.Builder;

namespace MediatR.Extras
{
    public static class QueryRegistrations
    {
        public static ContainerBuilder RegisterQueryHandler<THandler, TRequest, TReturn>(this ContainerBuilder builder)
            where THandler : IRequestHandler<TRequest, TReturn>
            where TRequest : IRequest<TReturn>
        {
            builder.RegisterQueryHandler<THandler, TRequest, TReturn>(
                x => x.RegisterType<THandler>());

            return builder;
        }

        public static ContainerBuilder RegisterQueryHandler<THandler, TRequest, TReturn>(this ContainerBuilder builder,
            Func<IComponentContext, THandler> handlerBuilder)
            where THandler : IRequestHandler<TRequest, TReturn>
            where TRequest : IRequest<TReturn>
        {
            builder.RegisterQueryHandler<THandler, TRequest, TReturn>(
                x => x.Register(handlerBuilder));

            return builder;
        }

        public static ContainerBuilder RegisterQueryHandler<TRequest, TReturn>(this ContainerBuilder builder, Func<TRequest, TReturn> handler)
            where TRequest : IRequest<TReturn>
        {
            builder.RegisterQueryHandler<RequestDelegateWrapper<TRequest, TReturn>, TRequest, TReturn>(
                x => x.Register(c => new RequestDelegateWrapper<TRequest, TReturn>(handler)));

            return builder;
        }

        public static ContainerBuilder RegisterQueryHandler<TRequest, TReturn>(this ContainerBuilder builder,
            Func<IComponentContext, Func<TRequest, TReturn>> handlerBuilder)
            where TRequest : IRequest<TReturn>
        {
            builder.RegisterQueryHandler<RequestDelegateWrapper<TRequest, TReturn>, TRequest, TReturn>(
                x => x.Register(c =>
                {
                    var handler = handlerBuilder(c);
                    return new RequestDelegateWrapper<TRequest, TReturn>(handler);
                }));

            return builder;
        }

        static void RegisterQueryHandler<THandler, TRequest, TReturn>(
            this ContainerBuilder builder, Func<ContainerBuilder, IRegistrationBuilder<THandler, IConcreteActivatorData, SingleRegistrationStyle>> registration)
            where THandler : IRequestHandler<TRequest, TReturn>
            where TRequest : IRequest<TReturn>
        {
            registration(builder)
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<THandler>("inner");

            builder.Register(c =>
            {
                var handler = c.ResolveNamed<THandler>("inner");
                return new QueryHandler<TRequest, TReturn>(handler);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();
        }
    }

    public static class CommandRegistrations
    {
        public static ContainerBuilder RegisterCommandHandler<TRequest>(this ContainerBuilder builder, Action<TRequest> handler)
            where TRequest : IRequest<Unit>
        {
            builder.RegisterCommandHandler<RequestDelegateWrapper<TRequest>, TRequest>(
                x => x.Register(c => new RequestDelegateWrapper<TRequest>(handler)));

            return builder;
        }

        public static ContainerBuilder RegisterCommandHandler<TRequest>(this ContainerBuilder builder,
            Func<IComponentContext, Action<TRequest>> handlerBuilder)
            where TRequest : IRequest<Unit>
        {
            builder.RegisterCommandHandler<RequestDelegateWrapper<TRequest>, TRequest>(
                x => x.Register(c =>
                {
                    var handler = handlerBuilder(c);
                    return new RequestDelegateWrapper<TRequest>(handler);
                }));

            return builder;
        }

        public static ContainerBuilder RegisterCommandHandler<THandler, TRequest>(this ContainerBuilder builder)
            where TRequest : IRequest<Unit>
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder.RegisterCommandHandler<THandler, TRequest>(
                x => x.RegisterType<THandler>());

            return builder;
        }

        public static ContainerBuilder RegisterCommandHandler<THandler, TRequest>(this ContainerBuilder builder,
            Func<IComponentContext, THandler> handlerBuilder)
            where TRequest : IRequest<Unit>
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder.RegisterCommandHandler<THandler, TRequest>(
                x => x.Register(handlerBuilder));

            return builder;
        }

        static void RegisterCommandHandler<THandler, TRequest>(this ContainerBuilder builder,
            Func<ContainerBuilder, IRegistrationBuilder<THandler, IConcreteActivatorData, SingleRegistrationStyle>> registration)
            where THandler : IRequestHandler<TRequest, Unit>
            where TRequest : IRequest<Unit>
        {
            registration(builder)
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<THandler>("inner");

            builder.Register(c =>
            {
                IRequestHandler<TRequest, Unit> handler = c.ResolveNamed<THandler>("inner");
                return handler.GetType() == typeof(ExceptionLoggingHandler<TRequest, Unit>)
                        ? handler as ExceptionLoggingHandler<TRequest, Unit>
                        : new ExceptionLoggingHandler<TRequest, Unit>(handler);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();
        }
    }

    public static class NotificationRegistrations
    {
        public static ContainerBuilder RegisterEventHandler<TNotification>(this ContainerBuilder builder, Action<TNotification> handler)
            where TNotification : INotification
        {
            builder.RegisterNotificationHandler<NotificationsDelegateWrapper<TNotification>, TNotification>(
                x => x.Register(c => new NotificationsDelegateWrapper<TNotification>(handler)));

            return builder;
        }

        public static ContainerBuilder RegisterEventHandler<TNotification>(this ContainerBuilder builder, 
            Func<IComponentContext, Action<TNotification>> handlerBuilder)
            where TNotification : INotification
        {
            builder.RegisterNotificationHandler<NotificationsDelegateWrapper<TNotification>, TNotification>(
                x => x.Register(c =>
                {
                    var handler = handlerBuilder(c);
                    return new NotificationsDelegateWrapper<TNotification>(handler);
                }));

            return builder;
        }

        public static ContainerBuilder RegisterEventHandler<THandler, TNotification>(this ContainerBuilder builder)
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
        {
            builder.RegisterNotificationHandler<THandler, TNotification>(
                x => x.RegisterType<THandler>());

            return builder;
        }

        public static ContainerBuilder RegisterEventHandler<THandler, TNotification>(this ContainerBuilder builder, 
            Func<IComponentContext, THandler> handlerBuilder)
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
        {
            builder.RegisterNotificationHandler<THandler, TNotification>(
                x => x.Register(handlerBuilder));

            return builder;
        }

        static void RegisterNotificationHandler<THandler, TNotification>(this ContainerBuilder builder,
            Func<ContainerBuilder, IRegistrationBuilder<THandler, IConcreteActivatorData, SingleRegistrationStyle>> registration)
            where THandler : INotificationHandler<TNotification> 
            where TNotification : INotification
        {
            registration(builder)
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<THandler>("inner");

            builder
                .Register(c =>
                {
                    INotificationHandler<TNotification> handler = c.ResolveNamed<THandler>("inner");
                    return handler.GetType() == typeof(ExceptionLoggingHandler<TNotification>)
                        ? handler as ExceptionLoggingHandler<TNotification>
                        : new ExceptionLoggingHandler<TNotification>(handler);
                })
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();
        }
    }
}