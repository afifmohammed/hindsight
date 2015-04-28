using System;

namespace MediatR.Extras
{
    public class PolarIceMelts : INotification
    {
        public DateTime When { get; set; }
    }

    public class ClimateChange : INotification
    {
        public DateTime When { get; set; }
    }
}