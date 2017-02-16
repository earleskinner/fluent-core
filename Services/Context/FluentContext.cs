using System;

namespace Fluent.Core.Services.Context
{
    public class FluentContext : IFluentContext
    {
        public Guid CorrelationId { get; set; }
    }
}