using System.Collections.Generic;
using System.Linq;

namespace MediatR.Extras
{
    public class ListOfHandlers<TNotification> : INotificationHandler<TNotification> where TNotification : INotification
    {
        private readonly IEnumerable<INotificationHandler<TNotification>> handlers;

        public ListOfHandlers(IEnumerable<INotificationHandler<TNotification>> handlers)
        {
            this.handlers = handlers.Select(x => x is ExceptionLoggingHandler<TNotification> 
                ? x 
                : new ExceptionLoggingHandler<TNotification>(x));
        }

        public void Handle(TNotification notification)
        {
            foreach (var handler in this.handlers)
            {
                using (new Benchmarker(handler.ToString(), notification))
                    handler.Handle(notification);
            }
        }

        public override string ToString()
        {
            return this.handlers.Where(x => x != null).Select(x => x.ToString()).Aggregate((m, n) => m + "," + n);
        }
    }
}