using System;
using System.Linq;
using System.Reflection;
using Autofac;
using MediatR.Extras;
using MediatR.Sagas;

namespace MediatR.Hangfire
{
    public static class Registrations
    {
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

            builder.RegisterType<EnqueueHangfireJobForEventHandler<THandler, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.Register(c =>
            {
                var handler = c.Resolve<EnqueueHangfireJobForEventHandler<THandler, TNotification>>();
                var queue = c.Resolve<Queue>();
                return new EnqueueEventHandler<TNotification>(handler, queue);
            })
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

            builder.RegisterType<EnqueueHangfireJobForEventHandler<THandler, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.Register(c =>
            {
                var handler = c.Resolve<EnqueueHangfireJobForEventHandler<THandler, TNotification>>();
                var queue = c.Resolve<Queue>();
                return new EnqueueEventHandler<TNotification>(handler, queue);
            })
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

            builder.RegisterType<EnqueueHangfireJobForRequestHandler<THandler, TRequest>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.Register(c =>
            {
                var handler = c.Resolve<EnqueueHangfireJobForRequestHandler<THandler, TRequest>>();
                var queue = c.Resolve<Queue>();
                return new EnqueueRequestHandler<TRequest>(handler, queue);
            })
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

            builder.RegisterType<EnqueueHangfireJobForRequestHandler<THandler, TRequest>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.Register(c =>
            {
                var handler = c.Resolve<EnqueueHangfireJobForRequestHandler<THandler, TRequest>>();
                var queue = c.Resolve<Queue>();
                return new EnqueueRequestHandler<TRequest>(handler, queue);
            })
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

            builder.RegisterType<EnqueueHangfireJobForRequestHandler<RequestDelegateWrapper<TRequest>, TRequest>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder.Register(c =>
            {
                var handler = c.Resolve<EnqueueHangfireJobForRequestHandler<RequestDelegateWrapper<TRequest>, TRequest>>();
                var queue = c.Resolve<Queue>();
                return new EnqueueRequestHandler<TRequest>(handler, queue);
            })
            .InstancePerLifetimeScope()
            .PropertiesAutowired()
            .AsImplementedInterfaces();

            return builder;
        }

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
                typeof(Registrations)
                    .GetMethod("RegisterEnqueuedSagaNotificationHandler", BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(typeof(TSagaHandler), typeof(TSagaState), notificationType)
                    .Invoke(null, new object[] { container });
            }

            return container;
        }

        /// <remarks> Invoked via reflection, do not delete </remarks>
        static ContainerBuilder RegisterEnqueuedSagaNotificationHandler<TSagaHandler, TSagaState, TNotification>(
            this ContainerBuilder container)
            where TSagaState : class, ISagaState, new()
            where TSagaHandler : SagaOf<TSagaState>, INotificationHandler<TNotification>
            where TNotification : INotification
        {
            container
                .RegisterType<SagaNotificationHandler<TNotification, TSagaHandler, TSagaState>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            container
                .RegisterType<Scoped<SagaNotificationHandler<TNotification, TSagaHandler, TSagaState>, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            container
                .RegisterType<EnqueueHangfireJobForEventHandler<SagaNotificationHandler<TNotification, TSagaHandler, TSagaState>, TNotification>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            container
                .Register(c =>
                {
                    var handler = c.Resolve<EnqueueHangfireJobForEventHandler<SagaNotificationHandler<TNotification, TSagaHandler, TSagaState>, TNotification>>();
                    var queue = c.Resolve<Queue>();
                    return new EnqueueEventHandler<TNotification>(handler, queue);
                })
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return container;
        }
    }
}