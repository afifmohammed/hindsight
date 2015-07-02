using System;
using System.Collections.Generic;
using MediatR.Extras;

namespace MediatR.Sagas
{
    class PaymentSubmitted : INotification
    {
        #region Public Members

        public String Token { get; set; }
        public Int32 Order { get; set; }
        public Decimal Amount { get; set; }

        #endregion
    }

    class StockPendingReserved : ITimeout, INotification
    {
        #region Interface Implementations

        public TimeSpan Interval { get; set; }

        #endregion

        #region Public Members

        public Int32 Order { get; set; }

        #endregion
    }

    class StockReserved : INotification
    {
        #region Public Members

        public Int32 Order { get; set; }

        #endregion
    }

    class PaymentApproved : INotification
    {
        #region Public Members

        public Int32 Order { get; set; }

        #endregion
    }

    class PaymentSagaState : ISagaState
    {
        #region Interface Implementations

        public IDictionary<String, Func<Boolean>> Invariants()
        {
            return new Dictionary<String, Func<Boolean>>
            {
                {typeof(PaymentSubmitted).Name, () => PaymentSubmitted},
                {typeof(StockReserved).Name, () => StockReserved}
            };
        }

        #endregion

        #region Public Members

        public Boolean PaymentSubmitted { get; set; }
        public Boolean StockReserved { get; set; }

        #endregion
    }

    class PaymentSaga : SagaOf<PaymentSagaState>,
        INotificationHandler<PaymentSubmitted>,
        INotificationHandler<StockReserved>,
        INotificationHandler<StockPendingReserved>
    {
        #region Interface Implementations

        public void Handle(PaymentSubmitted notification)
        {
            TryComplete();
        }

        public void Handle(StockPendingReserved notification) {}

        public void Handle(StockReserved notification)
        {
            TryComplete();
        }

        #endregion

        #region Overrides

        protected override void ConfigureMessageMapping()
        {
            MapMessage<PaymentSubmitted>(x => x.Order, s => e => s.PaymentSubmitted = true);
            MapMessage<StockReserved>(x => x.Order, s => e => s.StockReserved = true);
            MapMessage<StockPendingReserved>(x => x.Order, s => e => { });
        }

        #endregion

        #region Private Members

        private void TryComplete()
        {
            Publish(new PaymentApproved
            {
                Order = Saga.Id
            });

            Saga.MarkedAsComplete = true;
        }

        #endregion
    }

    class MagicPaymentSaga : MagicSagaOf<PaymentSagaState>,
        INotificationHandler<PaymentSubmitted>,
        INotificationHandler<StockReserved>,
        INotificationHandler<StockPendingReserved>
    {
        protected override void ConfigureMessageMapping()
        {
            Register<PaymentSubmitted>(x => x.Order);
            Register<StockReserved>(x => x.Order);
            Register<StockPendingReserved>(x => x.Order);
        }
        
        public void Handle(PaymentSubmitted notification)
        {
            Saga.State.PaymentSubmitted = true;
        }

        public void Handle(StockReserved notification)
        {
            Saga.State.StockReserved = true;
        }

        public void Handle(StockPendingReserved notification)
        {
            //no-op
        }

        public override Boolean TryComplete()
        {
            Publish(new PaymentApproved
            {
                Order = Saga.Id
            });

            return (Saga.MarkedAsComplete = true);
        }
    }
}