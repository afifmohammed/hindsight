using Autofac;

namespace MediatR.Extras
{
    public class AuditNotificationsHandler<TNotification> : CanMediate, INotificationHandler<TNotification> where TNotification : INotification, ICorrelated
    {
        public void Handle(TNotification notification)
        {
            if (ReferenceEquals(notification, null)) return;
            if (!SendRequest(new Configured<AuditEvents, bool> {Default = true})) return;
            SendRequest(new Audit<TNotification> {Notification = notification});
        }
    }

    public class Audit<TNotification> : IRequest, ICorrelated
        where TNotification : INotification, ICorrelated
    {
        public TNotification Notification { get; set; }
        public string CorrelationId { get { return Notification.CorrelationId; } }
    }

    public struct AuditEvents { }

    public static class AuditRegistrations
    {
        public static ContainerBuilder RegisterAuditor<TNotification>(this ContainerBuilder container) where TNotification : INotification, ICorrelated
        {
            container.RegisterEventHandler<AuditNotificationsHandler<TNotification>, TNotification>();
            return container;
        }
    }
}