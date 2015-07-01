namespace MediatR.Hangfire
{
    class Beep : IRequest { }

    class BeepHandler : IRequestHandler<Beep, Unit>
    {
        public Unit Handle(Beep message)
        {
            return new Unit();
        }
    }

    class Pinged : INotification { }

    class PingedHandler : INotificationHandler<Pinged>
    {
        public void Handle(Pinged notification)
        {
            
        }
    }
}