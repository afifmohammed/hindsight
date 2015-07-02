using System;
using System.Linq;
using System.Reflection;
using Autofac;
using MediatR.Extras;

namespace MediatR.Sagas
{
    public static class Registrations
    {
        public static ContainerBuilder RegisterSaga<TSagaHandler, TSagaState>(this ContainerBuilder container)
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
                    .AsImplementedInterfaces()
                    .PropertiesAutowired();

                if (typeof (ITimeout).IsAssignableFrom(notificationType))
                    container.RegisterType(typeof (TimeoutHandler<>).MakeGenericType(notificationType))
                        .InstancePerLifetimeScope()
                        .AsImplementedInterfaces()
                        .PropertiesAutowired();
            }

            return container;
        }

        public static ContainerBuilder RegisterMagicSaga<TSagaHandler, TSagaState>(this ContainerBuilder container)
            where TSagaState : class, ISagaState, new()
            where TSagaHandler : MagicSagaOf<TSagaState>
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
                var sagaNotificationHandlerType = typeof(MagicSagaNotificationHandler<,,>).MakeGenericType(
                                            notificationType,
                                            typeof(TSagaHandler),
                                            typeof(TSagaState));

                container.RegisterType(sagaNotificationHandlerType)
                    .InstancePerLifetimeScope()
                    .AsImplementedInterfaces()
                    .PropertiesAutowired();

                if (typeof(ITimeout).IsAssignableFrom(notificationType))
                    container.RegisterType(typeof(TimeoutHandler<>).MakeGenericType(notificationType))
                        .InstancePerLifetimeScope()
                        .AsImplementedInterfaces()
                        .PropertiesAutowired();
            }

            return container;
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

                if (typeof(ITimeout).IsAssignableFrom(notificationType))
                {
                    var handlerType = typeof(TimeoutHandler<>).MakeGenericType(notificationType);
                    var requestType = typeof(Timeout<>).MakeGenericType(notificationType);

                    typeof(Extras.Registrations)
                        .GetMethod("RegisterScheduledCommandHandler", BindingFlags.Public | BindingFlags.Static)
                        .MakeGenericMethod(handlerType, requestType)
                        .Invoke(null, new object[] { container });
                }
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
            container.RegisterEnqueuedEventHandler<SagaNotificationHandler<TNotification, TSagaHandler, TSagaState>, TNotification>(c =>
            {
                var handler = c.ResolveNamed<TSagaHandler>("inner");
                return new SagaNotificationHandler<TNotification, TSagaHandler, TSagaState>(handler);
            });

            return container;
        }
    }
}