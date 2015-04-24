using System;

namespace MediatR.Extras
{
    public abstract class Notification<TContent> : INotification
    {
        protected Notification()
        {
            When = DateTimeOffset.Now;
        }

        public TContent Content { get; set; }
        public DateTimeOffset When { get; set; }

        public override string ToString()
        {
            return Content.ToString();
        }
    }

    public abstract class Command<TRequest> : IRequest
    {
        public virtual TRequest Content { get; set; }

        public override string ToString()
        {
            return Content.ToString();
        }
    }

    /// <summary>
    /// Can be a command or a query, as some commands (not ideal) return a response
    /// </summary>
    public abstract class Request<TRequest, TResponse> : IRequest<TResponse>
    {
        public virtual TRequest Content { get; set; }

        public override string ToString()
        {
            return Content.ToString();
        }
    }
}