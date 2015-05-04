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

        public static ContainerBuilder RegisterEnqueuedCommandHandler<TRequest>(this ContainerBuilder builder, Action<TRequest> handler)
            where TRequest : IRequest<Unit>
        {
            builder.RegisterEnqueuedCommandHandler(c => handler);
            return builder;
        }

        public static ContainerBuilder RegisterEnqueuedCommandHandler<THandler, TRequest>(this ContainerBuilder builder)
            where TRequest : IRequest<Unit> 
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder.RegisterType<THandler>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<THandler>("inner");

            builder.RegisterEnqueuedInnerRequestHandler<THandler, TRequest>();

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
                .Named<THandler>("inner");

            builder.RegisterEnqueuedInnerRequestHandler<THandler, TRequest>();

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
            .Named<RequestDelegateWrapper<TRequest>>("inner");

            builder.RegisterEnqueuedInnerRequestHandler<RequestDelegateWrapper<TRequest>, TRequest>();

            return builder;
        }

        public static ContainerBuilder RegisterEnqueuedSaga<TSagaHandler, TSagaState>(this ContainerBuilder container)
            where TSagaState : class, ISagaState, new()
            where TSagaHandler : SagaOf<TSagaState>
        {
            container.RegisterType<TSagaHandler>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<TSagaHandler>("inner");

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
                .Register(c =>
                {
                    var handler = c.ResolveNamed<TSagaHandler>("inner");
                    return new SagaNotificationHandler<TNotification, TSagaHandler, TSagaState>(handler);
                })
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<SagaNotificationHandler<TNotification, TSagaHandler, TSagaState>>("inner");

            container.RegisterEnqueuedInnerNotificationHandler<SagaNotificationHandler<TNotification, TSagaHandler, TSagaState>, TNotification>();

            return container;
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

        static ContainerBuilder RegisterEnqueuedInnerRequestHandler<THandler, TRequest>(this ContainerBuilder builder)
            where TRequest : IRequest<Unit>
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder
                .RegisterType<Scoped<THandler, TRequest, Unit>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsSelf();

            builder
                .Register(c =>
                {
                    var handler = c.ResolveNamed<THandler>("inner");
                    return new SendEnqueueRequestForCommandHandler<THandler, TRequest>(handler);
                })
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<SendEnqueueRequestForCommandHandler<THandler, TRequest>>("inner");

            builder
                .Register(c =>
                {
                    var handler = c.ResolveNamed<SendEnqueueRequestForCommandHandler<THandler, TRequest>>("inner");
                    var queue = c.Resolve<Queue>();
                    return new EnqueueRequestHandler<TRequest>(handler, queue);
                })
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            builder
                .RegisterType<EnqueueHangfireJobForCommandHandler<THandler, TRequest>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }

    }
}