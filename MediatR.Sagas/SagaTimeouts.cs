﻿using System;

namespace MediatR.Sagas
{
    public class Timeout<TNotification> : IRequest 
        where TNotification : class, INotification, new()
    {
        public Timeout()
        {
            Interval = TimeSpan.FromSeconds(5);
            Notification = new TNotification();
        }

        public TNotification Notification { get; set; }
        public TimeSpan Interval { get; set; }
    }

    public abstract partial class SagaOf<TSagaState> 
    {
        protected void RequestTimeout<TNotification>(TimeSpan timespan, TNotification notification = null) 
            where TNotification : class, INotification, new()
        {
            var message = notification ?? new TNotification();

            SendRequest(new Timeout<TNotification>
            {
                Interval = timespan,
                Notification = message
            });
        }
    }
}