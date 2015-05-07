using System;
using System.Collections.Generic;
using Hangfire;
using MediatR.Extras;

namespace MediatR.Hangfire
{
    class Enqueue<THandler, TRequest, TReturn> : Request<TRequest, TReturn>
        where TRequest : IRequest<TReturn>
        where THandler : IRequestHandler<TRequest, TReturn>
    {
        public Type Handler { get { return typeof(THandler); } }
    }

    class EnqueueHangfireJobForCommandHandler<THandler, TRequest> : CanMediate,
        IRequestHandler<Enqueue<THandler, TRequest, Unit>, Unit>
        where TRequest : IRequest<Unit>
        where THandler : IRequestHandler<TRequest, Unit>
    {
        public Unit Handle(Enqueue<THandler, TRequest, Unit> message)
        {
            var request = message.Content;
            BackgroundJob.Enqueue<Scoped<THandler, TRequest, Unit>>(x => x.Handle(request));
            return new Unit();
        }
    }

}