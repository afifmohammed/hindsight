using System;
using System.Threading.Tasks;

namespace MediatR.Extras
{
    public class BenchmarkedMediator : IMediator
    {
        private readonly IMediator mediator;
        private readonly string handler;

        public BenchmarkedMediator(IMediator mediator, object handler)
        {
            this.mediator = mediator;
            this.handler = handler.ToString();
        }

        public TResponse Send<TResponse>(IRequest<TResponse> request)
        {
            using (new Benchmarker(this.handler, request))
                return this.mediator.Send(request);
        }

        public Task<TResponse> SendAsync<TResponse>(IAsyncRequest<TResponse> request)
        {
            using (new Benchmarker(this.handler, request))
                return this.mediator.SendAsync(request);
        }

        public void Publish(INotification notification)
        {
            using (new Benchmarker(this.handler, notification))
                this.mediator.Publish(notification);
        }

        public Task PublishAsync(IAsyncNotification notification)
        {
            using (new Benchmarker(this.handler, notification))
                return this.mediator.PublishAsync(notification);
        }
    }

    public abstract class CanMediate
    {
        private IMediator mediator;
        public IMediator Mediator
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", string.Format("Cannot provide a NULL Mediator to '{0}'", this));
                this.mediator = new BenchmarkedMediator(value, this);
            }
        }

        protected TResult SendRequest<TResult>(IRequest<TResult> request)
        {
            EnsureCanMediate();
            return this.mediator.Send(request);
        }

        protected void SendCommand<TRequest1>(TRequest1 request) where TRequest1 : IRequest
        {
            EnsureCanMediate();
            this.mediator.Send(request);
        }

        protected Func<TRequest1, TResponse1> GetFunction<TRequest1, TResponse1>(Func<IMediator, Func<TRequest1, TResponse1>> request)
        {
            EnsureCanMediate();
            return request(this.mediator);
        }

        protected Action<TRequest1> GetFunction<TRequest1>(Func<IMediator, Action<TRequest1>> request)
        {
            EnsureCanMediate();
            return request(this.mediator);
        }

        protected void Publish<TNotification1>(TNotification1 notification) where TNotification1 : INotification
        {
            EnsureCanMediate();
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