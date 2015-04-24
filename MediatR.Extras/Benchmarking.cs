using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using MediatR.Extras.Logging;
using Newtonsoft.Json;

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
                this.input != null ? this.input.ToString() : null);
        }

        private void LogMessage()
        {
            log.Log(LogLevel.Info, () => "{ElapsedMilliseconds} ms for {Handler} to process {Message} for {@Content}", null,
                this.watch.Elapsed.TotalMilliseconds,
                this.handler,
                this.input != null ? this.input.GetType().CSharpName() : null,
                this.input != null ? this.input.ToString() : null);
        }

        class Message
        {
            private readonly object value;

            public Message(object value)
            {
                this.value = value;
            }

            public override string ToString()
            {
                var kvps = ToExpando(this.value);

                var message = JsonConvert.SerializeObject(kvps);

                return message;
            }

            private static object ToExpando(object value)
            {
                IDictionary<string, object> ids = new ExpandoObject();
                IDictionary<string, object> all = new ExpandoObject();

                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                {
                    all.Add(property.Name, property.GetValue(value));
                    if (property.Name.ToLower().EndsWith("id"))
                        ids.Add(property.Name, property.GetValue(value));
                }

                var kvps = ids.Any() ? ids : all;
                return (kvps.Count == 1 ? kvps.First().Value : kvps);
            }
        }
    }
}