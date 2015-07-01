using System;
using Autofac;

namespace MediatR.Extras
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
                .Named<THandler>("handler");

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
                .Named<THandler>("handler");

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
            .Named<RequestDelegateWrapper<TRequest>>("handler");

            builder.RegisterEnqueuedInnerCommandHandler<RequestDelegateWrapper<TRequest>, TRequest>();

            return builder;
        }

        static ContainerBuilder RegisterEnqueuedInnerCommandHandler<THandler, TRequest>(this ContainerBuilder builder)
            where TRequest : IRequest<Unit>
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder
                .Register(c =>
                {
                    var queue = c.Resolve<Queue>();
                    var handler = new SendEnqueueRequestForCommandHandler<THandler, TRequest>(c.ResolveNamed<THandler>("handler"));
                    c.InjectProperties(handler);
                    return new EnqueueRequestHandler<TRequest>(handler, queue);
                })
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }
    }
}