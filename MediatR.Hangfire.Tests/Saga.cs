using System;
using System.Collections.Generic;
using MediatR.Sagas;

namespace MediatR.Hangfire
{
    class PaymentSubmitted : INotification
    {
        public string Token { get; set; }
        public int Order { get; set; }
        public decimal Amount { get; set; }
    }

    class StockReserved : INotification
    {
        public int Order { get; set; }
    }

    class PaymentApproved : INotification
    {
        public int Order { get; set; }
    }

    class PaymentSagaState : ISagaState
    {
        public bool PaymentSubmitted { get; set; }
        public bool StockReserved { get; set; }

        public IDictionary<string, Func<bool>> Invariants()
        {
            return new Dictionary<string, Func<bool>>
            {
                {typeof(PaymentSubmitted).Name, () => PaymentSubmitted},
                {typeof(StockReserved).Name, () => StockReserved},
            };
        }
    }

    class PaymentPendingStock : INotification, ITimeout
    {
        public int Order { get; set; }
        public TimeSpan Interval { get { return TimeSpan.FromDays(3); } }
    }

    class PaymentSaga : SagaOf<PaymentSagaState>, INotificationHandler<PaymentSubmitted>, INotificationHandler<StockReserved>, INotificationHandler<PaymentPendingStock>
    {
        protected override void ConfigureMessageMapping()
        {
            MapMessage<PaymentSubmitted>(x => x.Order, s => e => s.PaymentSubmitted = true);
            MapMessage<StockReserved>(x => x.Order, s => e => s.StockReserved = true);
            MapMessage<PaymentPendingStock>(x => x.Order, s => e => { });
        }

        public void Handle(PaymentSubmitted notification)
        {
            TryComplete();
        }

        public void Handle(StockReserved notification)
        {
            TryComplete();
        }

        private void TryComplete()
        {
            Publish(new PaymentApproved { Order = Saga.Id });
            Saga.MarkedAsComplete = true;
        }

        public void Handle(PaymentPendingStock notification)
        {}
    }
}