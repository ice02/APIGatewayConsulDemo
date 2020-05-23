using OpenTracing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.Tracing.Jaeger.Framework
{
    public interface IJaegerServiceTracer
    {
        ITracer Tracer { get; }
        string ServiceName { get; }
        string Environment { get; }
        string Identity { get; }

        ISpan Start(ISpanBuilder spanBuilder);
    }
}
