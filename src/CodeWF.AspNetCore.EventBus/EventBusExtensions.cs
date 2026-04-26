using CodeWF.EventBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace CodeWF.AspNetCore.EventBus
{
    public static class EventBusExtensions
    {
        private static Assembly[] GetAssemblies(Assembly[] assemblies)
        {
            return assemblies
                .Concat(new[] { Assembly.GetCallingAssembly() })
                .Distinct()
                .ToArray();
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddSingleton<IEventBus, CodeWF.EventBus.EventBus>();

            var allAssemblies = GetAssemblies(assemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => services.AddScoped(type),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                allAssemblies);

            return services;
        }

        public static void UseEventBus(this IApplicationBuilder app, params Assembly[] assemblies)
        {
            if (app.ApplicationServices.GetService<IEventBus>() is not { } messenger)
            {
                throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
            }

            var allAssemblies = GetAssemblies(assemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => messenger.Subscribe(type),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);
            messenger.RegisterServiceHandlerAction((type, action) =>
            {
                using var scope = app.ApplicationServices.CreateScope();
                var obj = scope.ServiceProvider.GetRequiredService(type);
                action(obj);
            });
            messenger.Subscribe(allAssemblies);
        }
    }
}
