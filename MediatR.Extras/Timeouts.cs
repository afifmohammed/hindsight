using System;

namespace MediatR.Extras
{
    public interface ITimeout
    {
        TimeSpan Interval { get; }
    }

    public class TimeoutHandler<TNotification> : IRequestHandler<Timeout<TNotification>, Unit>
        where TNotification : class, INotification, ITimeout, new()
    {
        private readonly IMediator mediator;

        public TimeoutHandler(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public Unit Handle(Timeout<TNotification> message)
        {
            this.mediator.Publish(message.Notification);
            return new Unit();
        }
    }

    public class Timeout<TNotification> : IRequest, ITimeout
        where TNotification : class, INotification, ITimeout, new()
    {
        public Timeout()
        {
            Notification = new TNotification();
        }

        public TNotification Notification { get; set; }
        public TimeSpan Interval { get { return Notification.Interval; } }
    }
}