using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fluent.Core.Validation
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ValidationType
    {
        [EnumMember(Value = "system_error")]
        System
    }
}