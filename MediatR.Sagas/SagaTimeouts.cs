using System;
using System.Threading.Tasks;
using MediatR.Extras;

namespace MediatR.Sagas
{
    public abstract partial class SagaOf<TSagaState> 
    {
        protected void RequestTimeout<TNotification>(TNotification notification = null) 
            where TNotification : class, ITimeout, INotification, new()
        {
            var message = notification ?? new TNotification();

            SendRequest(new Timeout<TNotification>
            {
                Notification = message
            });
        }
    }
}