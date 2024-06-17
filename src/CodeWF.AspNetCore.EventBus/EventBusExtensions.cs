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

            HandleCommandObject(type => services.AddSingleton(type),
                assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray());

            return services;
        }

        public static void UseEventBus(this IApplicationBuilder app, params Assembly[] assemblies)
        {
            if (app.ApplicationServices.GetService<IEventBus>() is not { } messenger)
            {
                throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
            }

            HandleCommandObject(type => messenger.Subscribe(app.ApplicationServices.GetService(type)),
                assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray());
        }

        private static void HandleCommandObject(Action<Type> handleRecipient, Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass
                                && !t.IsAbstract
                                && t.GetCustomAttributes<EventAttribute>().Any()
                                && t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Any(m =>
                                        m.GetCustomAttributes<EventHandlerAttribute>().Any()));

                foreach (var type in types)
                {
                    handleRecipient(type);
                }
            }
        }
    }
}