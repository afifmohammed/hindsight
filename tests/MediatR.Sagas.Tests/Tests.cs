using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using MediatR.Extras;
using Xunit;

namespace MediatR.Sagas
{
    public class CelebrationSagaTest
    {
        private readonly List<LetsCelebrate> celebrating = new List<LetsCelebrate>();
        private readonly IDictionary<int, SagaData<CelebrationSagaData>> sagas = new Dictionary<int, SagaData<CelebrationSagaData>>();
        private readonly ILifetimeScope scope;
        
        public CelebrationSagaTest()
        {
            this.scope = new ContainerBuilder()
                .RegisterSaga<CelebrationSaga, CelebrationSagaData>()
                .RegisterCommandHandler<LetsCelebrate>(celebrating.Add)
                .RegisterCommandHandler<Upsert<CelebrationSagaData>>(x =>
                {
                    if (sagas.ContainsKey(x.Content.Id))
                    {
                        sagas[x.Content.Id] = x.Content;
                        return;
                    }
                    sagas.Add(x.Content.Id, x.Content);
                })
                .RegisterQueryHandler<Load<CelebrationSagaData>, SagaData<CelebrationSagaData>>(x =>
                {
                    SagaData<CelebrationSagaData> saga;
                    sagas.TryGetValue(x.Content, out saga);
                    return saga;
                })
                .Build();
        }

        [Fact]
        public void CanFindSubscribers()
        {
            Assert.IsType<SagaNotificationHandler<BeerIsHere, CelebrationSaga, CelebrationSagaData>>(
                scope.Resolve<IEnumerable<INotificationHandler<BeerIsHere>>>().Single());
            Assert.IsType<SagaNotificationHandler<ItsThatTime, CelebrationSaga, CelebrationSagaData>>(
                scope.Resolve<IEnumerable<INotificationHandler<ItsThatTime>>>().Single());
        }

        [Fact]
        public void WasToldToCelebrate()
        {
            this.scope.Run(
                    x => x.Publish(new BeerIsHere {Beer = "Coopers", Person = 1}),
                    x => x.Publish(new ItsThatTime {Event = "AFL Final", Person = 1}));

            Assert.True(celebrating.Count(x => x.Person == 1) == 1);
        }
    }

    class CelebrationSaga : SagaOf<CelebrationSagaData>, INotificationHandler<ItsThatTime>, INotificationHandler<BeerIsHere>
    {
        protected override void ConfigureMessageMapping()
        {
            MapMessage<ItsThatTime>(x => x.Person, s => e => s.Event = e.Event);
            MapMessage<BeerIsHere>(x => x.Person, s => e => s.Beer = e.Beer);
        }

        public void Handle(ItsThatTime notification)
        {
            TryComplete();
        }

        public void Handle(BeerIsHere notification)
        {
            TryComplete();
        }

        private void TryComplete()
        {
            SendRequest(new LetsCelebrate
            {
                Beer = this.Saga.State.Beer,
                Person = this.Saga.Id,
                Event = this.Saga.State.Event
            });

            Saga.MarkedAsComplete = true;
        }
    }

    class CelebrationSagaData : ISagaState
    {
        public string Event { get; set; }
        public string Beer { get; set; }

        public IDictionary<string, Func<bool>> Invariants()
        {
            return new Dictionary<string, Func<bool>>
            {
                {typeof(ItsThatTime).Name, () => !string.IsNullOrEmpty(this.Event)},
                {typeof(BeerIsHere).Name, () => !string.IsNullOrEmpty(this.Beer)}
            };
        }
    }

    class ItsThatTime : INotification, ICorrelated
    {
        public ItsThatTime()
        {
            When = DateTimeOffset.Now;
        }
        public int Person { get; set; }
        public string Event { get; set; }
        public DateTimeOffset When { get; set; }
        public string CorrelationId { get { return Convert.ToString(Person); } }
    }

    class BeerIsHere : INotification, ICorrelated
    {
        public BeerIsHere()
        {
            When = DateTimeOffset.Now;
        }
        public int Person { get; set; }
        public string Beer { get; set; }
        public DateTimeOffset When { get; set; }
        public string CorrelationId { get { return Convert.ToString(Person); } }
    }

    class LetsCelebrate : IRequest
    {
        public int Person { get; set; }
        public string Event { get; set; }
        public string Beer { get; set; }
    }
}
