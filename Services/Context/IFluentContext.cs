using System;

namespace Fluent.Core.Services.Context
{
    public interface IFluentContext
    {
        Guid CorrelationId { get; set; }
    }
}