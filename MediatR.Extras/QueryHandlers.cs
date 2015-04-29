using System.Collections.Generic;
using MediatR.Extras.Logging;
using Newtonsoft.Json;

namespace MediatR.Extras
{
    class QueryHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _inner;
        private readonly IDictionary<string, TResponse> _responses = new Dictionary<string, TResponse>();
        private readonly ILog _log;

        public QueryHandler(IRequestHandler<TRequest, TResponse> inner)
        {
            _inner = inner;
            _log = LogProvider.GetLogger(inner.GetType().CSharpName());
        }

        public TResponse Handle(TRequest message)
        {
            TResponse response;
            var key = JsonConvert.SerializeObject(message);

            if (_responses.TryGetValue(key, out response))
            {
                var correlated = message as ICorrelated;
                if (correlated != null) _log.Log(LogLevel.Info, () => "{Message} with {CorrelationId} for {@Content} returns cached response", null, correlated, correlated.CorrelationId, key);
                if (correlated == null) _log.Log(LogLevel.Info, () => "{Message} for {@Content} returns cached response", null, message, key);

                return response;
            }

            response = _inner.Handle(message);
            _responses[key] = response;
            return response;
        }

        public override string ToString()
        {
            return this._inner.ToString();
        }
    }
}