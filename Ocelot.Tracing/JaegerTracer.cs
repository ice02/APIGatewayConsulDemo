using Jaeger;
using Microsoft.AspNetCore.Http;
using Ocelot.Tracing.Jaeger.Framework;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelot.Tracing
{
    public class JaegerTracer : DelegatingHandler, Logging.ITracer
    {
        private readonly OpenTracing.ITracer _tracer;
        private const string PrefixSpanId = "ot-spanId";

        public JaegerTracer(IServiceProvider services)
        {
            _tracer = services.GetService<IJaegerServiceTracer>();
        }

        public void Event(HttpContext httpContext, string @event)
        {
            // todo - if the user isnt using tracing the code gets here and will blow up on 
            // _tracer.Tracer.TryExtract..
            if (_tracer == null)
            {
                return;
            }

            var span = httpContext.GetSpan();

            if (span == null)
            {
                var spanBuilder =_tracer.BuildSpan($"server {httpContext.Request.Method} {httpContext.Request.Path}");
                if (_tracer.TryExtract(out var spanContext, httpContext.Request.Headers, (c, k) => c[k].GetValue(),
                    c => c.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.GetValue())).GetEnumerator()))
                {
                    spanBuilder.AsChildOf(spanContext);
                }

                span = _tracer.Start(spanBuilder);
                httpContext.SetSpan(span);
            }

            span?.Log(new LogData().Event(@event));
        }

        public Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken, 
            Action<string> addTraceIdToRepo, 
            Func<HttpRequestMessage, 
                CancellationToken, 
                Task<HttpResponseMessage>> baseSendAsync)
        {
            return _tracer.ChildTraceAsync($"httpclient {request.Method}", DateTimeOffset.UtcNow, span => TracingSendAsync(span, request, cancellationToken, addTraceIdToRepo, baseSendAsync));
        }

        protected virtual async Task<HttpResponseMessage> TracingSendAsync(
            OpenTracing.ISpan span,
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            Action<string> addTraceIdToRepo,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> baseSendAsync)
        {
            if (request.Headers.Contains(PrefixSpanId))
            {
                request.Headers.Remove(PrefixSpanId);
                request.Headers.TryAddWithoutValidation(PrefixSpanId, span.Context.SpanId);
            }

            addTraceIdToRepo(span.Context.TraceId);

            span.SetTag(Tags.SpanKind, Tags.SpanKindClient)
                .SetTag(Tags.HttpMethod, request.Method.Method)
                .SetTag(Tags.HttpUrl, request.RequestUri.ToString())
                //.SetTag(Tags.Host(request.RequestUri.Host)
                //.HttpPath(request.RequestUri.PathAndQuery)
                //.SetTag(Tags.PeerHostname(request.RequestUri.OriginalString)
                .SetTag(Tags.PeerHostname, request.RequestUri.Host)
                .SetTag(Tags.PeerPort, request.RequestUri.Port)
                ;

            var dictionary = new Dictionary<string, string>();

            _tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(dictionary));
            foreach (var entry in dictionary)
                request.Headers.Add(entry.Key, entry.Value);

            span.Log(LogField.CreateNew().ClientSend());

            var responseMessage = await baseSendAsync(request, cancellationToken);

            span.Log(LogField.CreateNew().ClientReceive());

            return responseMessage;
        }
    }
}
