using System;
using System.Linq;
using Autofac;
using MediatR.Extras;
using MediatR.Sagas;

namespace MediatR.Hangfire
{
    public static class Registrations
    {
        public static ContainerBuilder RegisterEnqueuedSaga<TSagaHandler, TSagaState>(this ContainerBuilder container)
            where TSagaState : class, ISagaState, new()
            where TSagaHandler : SagaOf<TSagaState>
        {
            container.RegisterType<TSagaHandler>()
                .AsSelf()
                .InstancePerLifetimeScope()
                .PropertiesAutowired();

            var contracts = typeof(TSagaHandler).GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>));

            foreach (var contract in contracts)
            {
                var notificationType = contract.GetGenericArguments().First();
                var sagaNotificationHandlerType = typeof(SagaNotificationHandler<,,>).MakeGenericType(
                                            notificationType,
                                            typeof(TSagaHandler),
                                            typeof(TSagaState));

                container.RegisterType(sagaNotificationHandlerType)
                    .InstancePerLifetimeScope()
                    .AsSelf()
                    .PropertiesAutowired();

                container.RegisterType(typeof(Scoped<,>).MakeGenericType(sagaNotificationHandlerType, notificationType))
                    .InstancePerLifetimeScope()
                    .AsSelf()
                    .PropertiesAutowired();

                container.RegisterType(typeof(EnqueueNotificationHandler<,>).MakeGenericType(sagaNotificationHandlerType, notificationType))
                    .InstancePerLifetimeScope()
                    .AsImplementedInterfaces()
                    .PropertiesAutowired();
            }

            return container;
        }

        public static ContainerBuilder RegisterEnqueuedEventHandler<THandler, TNotification>(this ContainerBuilder builder)
            where TNotification : INotification
            where THandler : INotificationHandler<TNotification>
        {
            builder.RegisterType<THandler>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.RegisterType<Scoped<THandler, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.RegisterType<EnqueueNotificationHandler<THandler, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

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
                .AsSelf();

            builder.RegisterType<Scoped<THandler, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.RegisterType<EnqueueNotificationHandler<THandler, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEnqueuedCommandHandler<THandler, TRequest>(this ContainerBuilder builder)
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
                .AsSelf();

            builder.RegisterType<EnqueueRequestHandler<THandler, TRequest>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEnqueuedCommandHandler<THandler, TRequest>(this ContainerBuilder builder,
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
                .AsSelf();

            builder.RegisterType<EnqueueRequestHandler<THandler, TRequest>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder RegisterEnqueuedCommandHandler<TRequest>(this ContainerBuilder builder, Action<TRequest> handler)
            where TRequest : IRequest<Unit>
        {
            builder.RegisterEnqueuedCommandHandler(c => handler);
            return builder;
        }

        public static ContainerBuilder RegisterEnqueuedCommandHandler<TRequest>(this ContainerBuilder builder, 
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
            .AsSelf();

            builder.RegisterType<Scoped<RequestDelegateWrapper<TRequest>, TRequest, Unit>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.RegisterType<EnqueueRequestHandler<RequestDelegateWrapper<TRequest>, TRequest>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }
    }
}