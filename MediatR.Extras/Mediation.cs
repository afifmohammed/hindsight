using System;

namespace MediatR.Extras
{
    public abstract class CanMediate
    {
        private IMediator mediator;

        public IMediator Mediator
        {
            set { this.mediator = value; }
        }

        protected TResult SendRequest<TResult>(IRequest<TResult> request)
        {
            EnsureCanMediate();
            using (new Benchmarker(this.ToString(), request))
                return this.mediator.Send(request);
        }

        protected void SendCommand<TRequest1>(TRequest1 request) where TRequest1 : IRequest
        {
            EnsureCanMediate();
            using (new Benchmarker(this.ToString(), request))
                this.mediator.Send(request);
        }

        protected Func<TRequest1, TResponse1> GetFunction<TRequest1, TResponse1>(Func<IMediator, Func<TRequest1, TResponse1>> request)
        {
            EnsureCanMediate();
            return r =>
            {
                using (new Benchmarker(this.ToString(), r))
                    return request(this.mediator)(r);
            };
        }

        protected Action<TRequest1> GetFunction<TRequest1>(Func<IMediator, Action<TRequest1>> request)
        {
            EnsureCanMediate();
            return r =>
            {
                using (new Benchmarker(this.ToString(), r))
                    request(this.mediator)(r);
            };
        }

        protected void Publish<TNotification1>(TNotification1 notification) where TNotification1 : INotification
        {
            EnsureCanMediate();
            using (new Benchmarker(this.ToString(), notification))
                this.mediator.Publish(notification);
        }

        private void EnsureCanMediate()
        {
            if (this.mediator == null)
                throw new InvalidOperationException(string.Format(" {0} cannot mediate without a Mediator, Did you forget to Autowire properties?", this));
        }

        public override string ToString()
        {
            return GetType().CSharpName();
        }
    }
}