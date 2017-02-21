using System;
using System.Collections.Generic;
using Fluent.Core.Services.Context;
using Fluent.Core.Validation;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fluent.Core.Api
{
    public class CommonResponses
    {
        private readonly IFluentContext _fluent;
        private readonly IHtmlLocalizer<CommonResponses> _localizer;

        public CommonResponses(IFluentContext fluent, IHtmlLocalizer<CommonResponses> localizer)
        {
            if (fluent == null)
            {
                throw new ArgumentNullException(nameof(fluent));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            _fluent = fluent;
            _localizer = localizer;
        }

        /// <summary>
        /// Example of a common response
        /// </summary>
        public BaseApiResponse DivideByZero => new BaseApiResponse
        {
            CorrelationId = _fluent.CorrelationId,
            Errors = new List<ValidationMessage>
            {
                new ValidationMessage
                {
                    Type = ValidationType.System,
                    Message = _localizer["division-by-zero"].Value,
                    Code = "divide_by_zero"
                }
            }
        };
    }

    public static class CommonResponsesExtensions
    {
        public static IServiceCollection AddCommonResponses(this IServiceCollection services)
        {
            services.TryAdd(new ServiceDescriptor(typeof(CommonResponses), typeof(CommonResponses), ServiceLifetime.Scoped));

            return services;
        }
    }
}