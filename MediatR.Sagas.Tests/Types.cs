using System;
using System.Collections.Generic;
using MediatR.Extras;

namespace MediatR.Sagas
{
    class PaymentSubmitted : INotification
    {
        public string Token { get; set; }
        public int Order { get; set; }
        public decimal Amount { get; set; }
    }

    class StockPendingReserved : ITimeout, INotification
    {
        public int Order { get; set; }
        public TimeSpan Interval { get; set; }
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

    class PaymentSaga : SagaOf<PaymentSagaState>, INotificationHandler<PaymentSubmitted>, INotificationHandler<StockReserved>, INotificationHandler<StockPendingReserved>
    {
        protected override void ConfigureMessageMapping()
        {
            MapMessage<PaymentSubmitted>(x => x.Order, s => e => s.PaymentSubmitted = true);
            MapMessage<StockReserved>(x => x.Order, s => e => s.StockReserved = true);
            MapMessage<StockPendingReserved>(x => x.Order, s => e => { });
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

        public void Handle(StockPendingReserved notification)
        {}
    }
}