﻿using System;
using Autofac;

namespace MediatR.Extras
{
    namespace Tests
    {
        public static class ContainerInvocationsUnderTest
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

                return r =>
                {
                    using (container)
                    using (var scope = container.BeginLifetimeScope())
                    using (var queue = scope.Resolve<Queue>())
                    {
                        try
                        {
                            var mediator = scope.Resolve<IMediator>();
                            return request(mediator)(r);
                        }
                        catch
                        {
                            queue.Clear();
                            throw;
                        }
                    }
                };

            }

            public static void Run(this ILifetimeScope container, params Action<IMediator>[] actions)
            {
                var builder = new ContainerBuilder();
                builder.Prepare(container);

                using (container)
                using (var scope = container.BeginLifetimeScope())
                using (var queue = scope.Resolve<Queue>())
                {
                    try
                    {
                        var mediator = scope.Resolve<IMediator>();

                        foreach (var action in actions)
                        {
                            action(mediator);
                        }
                    }
                    catch
                    {
                        queue.Clear();
                        throw;
                    }
                    
                }
            }

            public static void Notify<TNotification>(this ILifetimeScope container, TNotification notification)
                where TNotification : INotification
            {
                var builder = new ContainerBuilder();
                builder.Prepare(container);

                using (container)
                using (var scope = container.BeginLifetimeScope())
                using (var queue = scope.Resolve<Queue>())
                {
                    try
                    {
                        var mediator = scope.Resolve<IMediator>();
                        mediator.Publish(notification);
                    }
                    catch
                    {
                        queue.Clear();
                        throw;
                    }
                }
            }

            public static void Send(this ILifetimeScope container, params IRequest<Unit>[] commands)
            {
                var builder = new ContainerBuilder();
                builder.Prepare(container);

                using (container)
                using (var scope = container.BeginLifetimeScope())
                using (var queue = scope.Resolve<Queue>())
                {
                    try
                    {
                        foreach (var command in commands)
                        {
                            var mediator = scope.Resolve<IMediator>();
                            mediator.Send(command);
                        }
                    }
                    catch
                    {
                        queue.Clear();
                        throw;
                    }
                }

            }

            public static TResponse Request<TResponse>(this ILifetimeScope container, IRequest<TResponse> request)
            {
                var builder = new ContainerBuilder();

                builder.Prepare(container);

                using (container)
                using (var scope = container.BeginLifetimeScope())
                using (var queue = scope.Resolve<Queue>())
                {
                    try
                    {
                        var mediator = scope.Resolve<IMediator>();
                        return mediator.Send(request);
                    }
                    catch
                    {
                        queue.Clear();
                        throw;
                    }
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
    
}