using Newtonsoft.Json;

namespace Fluent.Core.Validation
{
    /// <summary>
    /// Validation message object
    /// </summary>
    public class ValidationMessage
    {
        /// <summary>
        /// The type of error returned
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// A human-readable message providing more details about the error
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// A tech-savvy code representing the error
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// The parameter the error relates to if the error is parameter-specific
        /// </summary>
        [JsonProperty("param")]
        public string Parameter { get; set; }
    }
}