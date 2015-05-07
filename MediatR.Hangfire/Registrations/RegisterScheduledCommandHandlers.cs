using System;
using Autofac;
using MediatR.Extras;
using MediatR.Sagas;

namespace MediatR.Hangfire
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
                .Named<THandler>("inner");

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
                .Named<THandler>("inner");

            builder.RegisterScheduledInnerRequestHandler<THandler, TRequest>();

            return builder;
        }

        static ContainerBuilder RegisterScheduledInnerRequestHandler<THandler, TRequest>(this ContainerBuilder builder)
            where TRequest : IRequest<Unit>, ITimeout
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
                    return new SendScheduleRequestForCommandHandler<THandler, TRequest>(handler);
                })
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .Named<SendScheduleRequestForCommandHandler<THandler, TRequest>>("inner");

            builder
                .Register(c =>
                {
                    var handler = c.ResolveNamed<SendScheduleRequestForCommandHandler<THandler, TRequest>>("inner");
                    var queue = c.Resolve<Queue>();
                    return new EnqueueRequestHandler<TRequest>(handler, queue);
                })
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            builder
                .RegisterType<ScheduleHangfireJobForCommandHandler<THandler, TRequest>>()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                .AsImplementedInterfaces();

            return builder;
        }
    }
}