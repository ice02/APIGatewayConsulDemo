﻿using Microsoft.AspNetCore.Http;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        private const string spanKey = "_request_span_";

        public static void SetSpan(this HttpContext httpContext, ISpan span)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            httpContext.Items[spanKey] = span;
        }

        public static ISpan GetSpan(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            return httpContext.Items[spanKey] as ISpan;
        }
    }
}
