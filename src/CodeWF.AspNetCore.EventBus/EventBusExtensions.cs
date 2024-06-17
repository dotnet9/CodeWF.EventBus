using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using CodeWF.EventBus;
using System.Linq;
using System;

namespace CodeWF.AspNetCore.EventBus
{
    public static class EventBusExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddSingleton<IEventBus, CodeWF.EventBus.EventBus>();

            var allAssemblies = assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray();

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => services.AddSingleton(type),
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

            var allAssemblies = assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray();

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(
                type => messenger.Subscribe(app.ApplicationServices.GetService(type)),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => messenger.Subscribe(type),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);
        }
    }
}