namespace Ocelot.Tracing.Jaeger
{
    using System;
    using DependencyInjection;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;

    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddJaeger(this IOcelotBuilder builder, Action<JaegerOptions> settings)
        {
            builder.Services.AddSingleton<ITracer, JaegerTracer>();
            builder.Services.AddJaeger(settings);
            return builder;
        }
    }
}
