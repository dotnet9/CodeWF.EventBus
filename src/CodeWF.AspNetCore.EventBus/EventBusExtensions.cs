using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CodeWF.EventBus
{
    public static partial class EventBusExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            AddEventBus(
                (t1, t2) => services.AddSingleton(t1, t2),
                t => services.AddSingleton(t),
                assemblies);

            return services;
        }

        public static void UseEventBus(this IApplicationBuilder app, params Assembly[] assemblies)
        {
            UseEventBus(t => app.ApplicationServices.GetRequiredService(t), assemblies);
        }
    }
}