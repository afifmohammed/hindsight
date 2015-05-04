using System;
using System.Diagnostics;
using MediatR.Extras.Logging;

namespace MediatR.Extras
{
    public class Benchmarker : IDisposable
    {
        private readonly string handler;
        private readonly object input;
        private readonly ILog log;
        private readonly Stopwatch watch;

        public Benchmarker(string handler, object input)
        {
            if (string.IsNullOrEmpty(handler))
                throw new ArgumentNullException("handler", "Cannot benchmark a NULL handler");

            this.log = LogProvider.GetLogger(handler);
            this.handler = handler;
            this.input = input;
            this.watch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            this.watch.Stop();
            var id = this.input as ICorrelated;
            if (id != null) LogCorrelatedMessage();
            if (id == null) LogMessage();
        }

        private void LogCorrelatedMessage()
        {
            log.Log(LogLevel.Info, () => "{ElapsedMilliseconds} ms for {Handler} to process {Message} with {CorrelationId} for {@Content}", null,
                this.watch.Elapsed.TotalMilliseconds,
                this.handler,
                this.input != null ? this.input.GetType().CSharpName() : null,
                this.input != null ? ((ICorrelated)this.input).CorrelationId : null,
                this.input);
        }

        private void LogMessage()
        {
            log.Log(LogLevel.Info, () => "{ElapsedMilliseconds} ms for {Handler} to process {Message} for {@Content}", null,
                this.watch.Elapsed.TotalMilliseconds,
                this.handler,
                this.input != null ? this.input.GetType().CSharpName() : null,
                this.input);
        }
    }
}