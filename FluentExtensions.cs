using System;
using System.Collections.Generic;
using System.Globalization;
using Fluent.Core.Api;
using Fluent.Core.Services.Context;
using Fluent.Core.Services.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fluent.Core
{
    public static class FluentExtensions
    {
        public static IServiceCollection AddFluent(this IServiceCollection services)
        {
            return AddFluent(services, null);
        }

        public static IServiceCollection AddFluent(this IServiceCollection services, Action<FluentOptions> setupAction)
        {
            // Add common responses
            services.AddCommonResponses();

            // Add Json Localization
            services.AddJsonLocalization(options =>
            {
                options.ResourcesPath = "Resources";
                options.AllowedAssembliesRegularExpression = "^fluent(.*)";
            });

            // Add fluent context
            services.AddFluentContext(setupAction: setupAction);

            // Configure framework services
            services.Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        new CultureInfo("en"),
                        new CultureInfo("en-GB"),
                        new CultureInfo("en-US")
                    };
                    options.DefaultRequestCulture = new RequestCulture(culture: "en", uiCulture: "en");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });

            return services;
        }

        public static IServiceCollection AddFluentContext(this IServiceCollection services, Action<FluentOptions> setupAction)
        {
            services.TryAdd(new ServiceDescriptor(typeof(IFluentContext), typeof(FluentContext), ServiceLifetime.Scoped));

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            return services;
        }
    }
}