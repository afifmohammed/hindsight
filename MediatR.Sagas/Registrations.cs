using System.Linq;
using Autofac;

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
            }

            return container;
        }
    }
}