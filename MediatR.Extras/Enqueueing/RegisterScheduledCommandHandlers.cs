using System;
using Autofac;

namespace MediatR.Extras
{
    public static partial class Registrations
    {
        public static ContainerBuilder RegisterScheduledCommandHandler<THandler, TRequest>(this ContainerBuilder builder)
            where TRequest : IRequest<Unit>, ITimeout
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder.RegisterType<THandler>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<THandler>("handler");

            builder.RegisterScheduledInnerRequestHandler<THandler, TRequest>();

            return builder;
        }

        internal static ContainerBuilder RegisterScheduledCommandHandler<THandler, TRequest>(this ContainerBuilder builder,
            Func<IComponentContext, THandler> handlerBuilder)
            where TRequest : IRequest<Unit>, ITimeout
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder.Register(handlerBuilder)
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<THandler>("handler");

            builder.RegisterScheduledInnerRequestHandler<THandler, TRequest>();

            return builder;
        }

        static ContainerBuilder RegisterScheduledInnerRequestHandler<THandler, TRequest>(this ContainerBuilder builder)
            where TRequest : IRequest<Unit>, ITimeout
            where THandler : IRequestHandler<TRequest, Unit>
        {
            builder
                .Register(c =>
                {
                    var queue = c.Resolve<Queue>();
                    var handler = new SendScheduleRequestForCommandHandler<THandler, TRequest>(c.ResolveNamed<THandler>("handler"));
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