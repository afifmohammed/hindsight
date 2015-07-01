using System;

namespace MediatR.Extras
{
    public class Schedule<THandler, TRequest, TReturn> : Request<TRequest, TReturn>
        where TRequest : IRequest<TReturn>
        where THandler : IRequestHandler<TRequest, TReturn>
    {
        public Type Handler { get { return typeof(THandler); } }
        public TimeSpan Interval { get; set; }
    }

    class SendScheduleRequestForCommandHandler<THandler, TRequest> : CanMediate,
        IRequestHandler<TRequest, Unit>
        where TRequest : IRequest<Unit>, ITimeout
        where THandler : IRequestHandler<TRequest, Unit>
    {
        private readonly THandler handler;

        public SendScheduleRequestForCommandHandler(THandler handler)
        {
            this.handler = handler;
        }

        public Unit Handle(TRequest request)
        {
            var enqueue = SendRequest(new Configured<EnqueueHandlers, bool> { Default = true });

            return enqueue
                ? SendRequest(new Schedule<THandler, TRequest, Unit> { Content = request, Interval = request.Interval })
                : this.handler.Handle(request);
        }
    }
}