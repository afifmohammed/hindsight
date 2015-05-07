using System;
using Autofac;
using MediatR.Extras;

namespace MediatR.Hangfire
{
    public static partial class Registrations
    {
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

            builder.RegisterEnqueuedInnerCommandHandler<THandler, TRequest>();

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

            builder.RegisterEnqueuedInnerCommandHandler<THandler, TRequest>();

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

            builder.RegisterEnqueuedInnerCommandHandler<RequestDelegateWrapper<TRequest>, TRequest>();

            return builder;
        }

        static ContainerBuilder RegisterEnqueuedInnerCommandHandler<THandler, TRequest>(this ContainerBuilder builder)
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