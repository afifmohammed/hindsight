using System;
using Autofac;
using MediatR.Extras;

namespace MediatR.Hangfire
{
    internal static class ContainerInvocations
    {
        public static IContainer Build(this ContainerBuilder builder, out ILifetimeScope scope)
        {
            var container = builder.Build();
            scope = container;
            return container;
        }
        
        public static ContainerBuilder With(this ContainerBuilder builder, Action<ContainerBuilder> action)
        {
            action(builder);
            return builder;
        }

        public static Func<TRequest1, TResponse1> Request<TRequest1, TResponse1>(this ILifetimeScope container,
            Func<IMediator, Func<TRequest1, TResponse1>> request)
        {
            var builder = new ContainerBuilder();
            builder.Prepare(container);

            var mediator = container.Resolve<IMediator>();
            return request(mediator);
        }

        public static void Run(this ILifetimeScope container, params Action<IMediator>[] actions)
        {
            var builder = new ContainerBuilder();
            builder.Prepare(container);

            using (container)
            {
                foreach (var action in actions)
                {
                    using (var scope = container.BeginLifetimeScope())
                    {
                        var mediator = scope.Resolve<IMediator>();
                        action(mediator);
                        var tasks = scope.Resolve<Tasks>();
                        tasks.ForEach(t => t());
                    }
                }
            }
        }

        public static void Notify<TNotification>(this ILifetimeScope container, TNotification notification)
            where TNotification : INotification
        {
            var builder = new ContainerBuilder();
            builder.Prepare(container);

            using (container)
            {
                var mediator = container.Resolve<IMediator>();
                mediator.Publish(notification);
            }
        }

        public static void Send(this ILifetimeScope container, params IRequest<Unit>[] commands)
        {
            var builder = new ContainerBuilder();
            builder.Prepare(container);

            using (container)
            {
                foreach (var command in commands)
                {
                    using (var scope = container.BeginLifetimeScope())
                    {
                        var mediator = scope.Resolve<IMediator>();
                        mediator.Send(command);
                    }
                }
            }
        }

        public static TResponse Request<TResponse>(this ILifetimeScope container, IRequest<TResponse> request)
        {
            var builder = new ContainerBuilder();

            builder.Prepare(container);

            using (container)
            {
                var mediator = container.Resolve<IMediator>();
                return mediator.Send(request);
            }
        }

        private static void Prepare(this ContainerBuilder builder, IComponentContext scope)
        {
            builder.RegisterModule<CanUseAMediator>();
            builder.RegisterModule<CanLookupConfiguredValues>();
            builder.Update(scope.ComponentRegistry);
        }
    }
}