using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Fluent.Core.Validation;

namespace Fluent.Core.Api
{
    /// <summary>
    /// Base class for api responses
    /// </summary>
    public class BaseApiResponse
    {
        /// <summary>
        /// Unique identifier of the request
        /// </summary>
        [JsonProperty("correlation_id")]
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// List of validation messages received during the request
        /// </summary>
        [JsonProperty("errors")]
        public IList<ValidationMessage> Errors { get; set; }
    }
}