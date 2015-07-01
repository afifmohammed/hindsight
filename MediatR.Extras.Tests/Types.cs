namespace MediatR.Extras
{
    struct MyConfigurationKey { }

    class Timezone : IRequest<string> { }

    class TimezoneProvider
    {
        public string Find(Timezone g)
        {
            return "AU";
        }
    }

    /// <remarks>
    /// this is a contrived example. 
    /// an abstraction over another abstraction is pointless.
    /// please do not use it as an example on how to design handlers. 
    /// </remarks>
    class TimezoneHandler : IRequestHandler<Timezone, string>
    {
        private readonly TimezoneProvider provider;

        public TimezoneHandler()
            : this(new TimezoneProvider())
        { }

        public TimezoneHandler(TimezoneProvider provider)
        {
            this.provider = provider;
        }

        public string Handle(Timezone message)
        {
            var timezone = this.provider.Find(message);
            return timezone;
        }
    }

    class OzyGreeting : IRequest { }

    class GreetingProvider
    {
        public void Greet(OzyGreeting g)
        { }
    }

    /// <remarks>
    /// this is a contrived example. 
    /// an abstraction over another abstraction is pointless.
    /// please do not use it as an example on how to design handlers. 
    /// </remarks>
    class GreetingHandler : IRequestHandler<OzyGreeting, Unit>
    {
        private readonly GreetingProvider provider;

        public GreetingHandler()
            : this(new GreetingProvider())
        { }

        public GreetingHandler(GreetingProvider provider)
        {
            this.provider = provider;
        }

        public Unit Handle(OzyGreeting message)
        {
            this.provider.Greet(message);
            return new Unit();
        }
    }

    class BatteryRunningLow : INotification { }

    class WifiProvider { public void Toggle(bool off) { } }
    class BlueToothProvider { public void Toggle(bool off) { } }

    /// <remarks>
    /// this is a contrived example. 
    /// an abstraction over another abstraction is pointless.
    /// please do not use it as an example on how to design handlers. 
    /// </remarks>
    class WifiHandler : INotificationHandler<BatteryRunningLow>
    {
        private readonly WifiProvider provider;
        public WifiHandler() : this(new WifiProvider()) { }
        public WifiHandler(WifiProvider provider) { this.provider = provider; }

        public void Handle(BatteryRunningLow notification)
        {
            this.provider.Toggle(off: true);
        }

        public override string ToString()
        {
            return GetType().CSharpName();
        }
    }

    /// <remarks>
    /// this is a contrived example. 
    /// an abstraction over another abstraction is pointless.
    /// please do not use it as an example on how to design handlers. 
    /// </remarks>
    class BlueToothHandler : INotificationHandler<BatteryRunningLow>
    {
        private readonly BlueToothProvider provider;
        public BlueToothHandler() : this(new BlueToothProvider()) { }
        public BlueToothHandler(BlueToothProvider provider) { this.provider = provider; }

        public void Handle(BatteryRunningLow notification)
        {
            this.provider.Toggle(off: true);
        }

        public override string ToString()
        {
            return GetType().CSharpName();
        }
    }

    class Beep : IRequest { }

    class BeepHandler : IRequestHandler<Beep, Unit>
    {
        public Unit Handle(Beep message)
        {
            return new Unit();
        }
    }

    class BurntOut : INotification { }
    class LayingLow : INotification { }
    class ManageBurnOuts : CanMediate, INotificationHandler<BurntOut>
    {
        public void Handle(BurntOut notification)
        {
            Publish(new LayingLow());
        }
    }

    class ManagerA : INotificationHandler<LayingLow>
    {
        public void Handle(LayingLow notification)
        { }
    }

    class ManagerB : INotificationHandler<LayingLow>
    {
        public void Handle(LayingLow notification)
        {}
    }
}