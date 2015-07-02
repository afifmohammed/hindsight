using Autofac;
using Xunit;

namespace MediatR.Sagas
{
    public class InOrderToCallTheSaga
    {
        readonly ILifetimeScope scope;

        public InOrderToCallTheSaga()
        {
            scope = new ContainerBuilder()
                .RegisterSaga<PaymentSaga, PaymentSagaState>()
                .Build();
        }

        [Fact]
        public void ShouldFindTheSagaInTheListOfSubscribers()
        {
            Assert.IsType<SagaNotificationHandler<PaymentSubmitted, PaymentSaga, PaymentSagaState>>(scope.Resolve<INotificationHandler<PaymentSubmitted>>());
            Assert.IsType<SagaNotificationHandler<StockReserved, PaymentSaga, PaymentSagaState>>(scope.Resolve<INotificationHandler<StockReserved>>());
        }
    }

    public class InOrderToCallTheMagicSaga
    {
        readonly ILifetimeScope scope;

        public InOrderToCallTheMagicSaga()
        {
            scope = new ContainerBuilder()
                .RegisterMagicSaga<MagicPaymentSaga, PaymentSagaState>()
                .Build();
        }

        [Fact]
        public void ShouldFindTheSagaInTheListOfSubscribers()
        {
            Assert.IsType<MagicSagaNotificationHandler<PaymentSubmitted, MagicPaymentSaga, PaymentSagaState>>(scope.Resolve<INotificationHandler<PaymentSubmitted>>());
            Assert.IsType<MagicSagaNotificationHandler<StockReserved, MagicPaymentSaga, PaymentSagaState>>(scope.Resolve<INotificationHandler<StockReserved>>());
        }
    }
}