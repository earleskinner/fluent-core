using System;
using System.Linq;
using Fluent.Core.Validation;

namespace Fluent.Core.Api
{
    /// <summary>
    /// Extension class for api requests and responses
    /// </summary>
    public static class ApiExtensions
    {
        public static BaseApiResponse ApplyParameter(this BaseApiResponse response, string parameter, Func<ValidationMessage, bool> predicate = null)
        {
            var messages = predicate == null ? response.Errors : response.Errors.Where(e => predicate(e));
            if (!messages.Any())
            {
                return response;
            }
            else
            {
                foreach (var message in messages)
                {
                    message.Parameter = parameter;
                }
            }
            return response;
        }
    }
}