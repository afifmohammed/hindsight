using System.Collections.Generic;

namespace MediatR.Extras
{
    public class NotifySubscribers<TNotification, TSubscriber> : CanMediate, INotificationHandler<TNotification> where TNotification : INotification
    {
        public void Handle(TNotification notification)
        {
            var subscribers = SendRequest(new Subscribers<TSubscriber> {Notification = notification});
            foreach (var subscriber in subscribers)
            {
                SendRequest(new Notify<TSubscriber>
                {
                    Notification = notification,
                    Subscriber = subscriber
                });
            }
        }
    }

    public class Subscribers<TSubscriber> : IRequest<IEnumerable<TSubscriber>>
    {
        public INotification Notification { get; set; }
    }

    public class Notify<TSubscriber> : IRequest
    {
        public INotification Notification { get; set; }
        public TSubscriber Subscriber { get; set; }
    }
}