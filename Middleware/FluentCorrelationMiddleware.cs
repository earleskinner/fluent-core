using System;
using System.Threading.Tasks;
using Fluent.Core.Services.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fluent.Core.Middleware
{
    /// <summary>
    /// Tries to capture any correlation id appended to the request for tracing purposes
    /// </summary>
    public class FluentCorrelationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FluentContext> _logger;
        private readonly string[] _headers = new [] { "X-Request-ID", "X-Correlation-ID" }; // See https://en.wikipedia.org/wiki/List_of_HTTP_header_fields

        public FluentCorrelationMiddleware(RequestDelegate next, ILogger<FluentContext> logger)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IFluentContext fluent)
        {
            var requestHeaders = context.Request.Headers;

            foreach (var header in _headers)
            {
                if (requestHeaders.ContainsKey(header))
                {
                    Guid headerValue;
                    if (Guid.TryParse(requestHeaders[header], out headerValue))
                    {
                        fluent.CorrelationId = headerValue;
                        _logger.LogInformation($"An existing Correlation Id {fluent.CorrelationId} was found in request header {header}");
                        break;
                    }
                }
            }

            // If a correlation id cannot be found in the request headers
            // Treat as new request with no prior dealings
            if (fluent.CorrelationId == Guid.Empty)
            {
                fluent.CorrelationId = Guid.NewGuid();
                _logger.LogInformation($"A new Correlation Id {fluent.CorrelationId} was generated for this request");
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}