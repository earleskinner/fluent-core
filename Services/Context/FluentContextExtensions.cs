using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fluent.Core.Services.Context
{
    public static class FluentContextExtensions
    {
        public static IServiceCollection AddFluentContext(this IServiceCollection services)
        {
            return AddFluentContext(services, setupAction: null);
        }

        public static IServiceCollection AddFluentContext(this IServiceCollection services, Action<FluentContextOptions> setupAction)
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