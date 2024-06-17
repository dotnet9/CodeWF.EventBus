using System;
using System.Linq;
using CodeWF.EventBus;
using DryIoc;
using System.Reflection;
using Prism.Ioc;

namespace CodeWF.DryIoc.EventBus
{
    public static class EventBusExtensions
    {
        public static IContainerRegistry AddEventBus(this IContainerRegistry services, params Assembly[] assemblies)
        {
            services.RegisterSingleton<IEventBus, CodeWF.EventBus.EventBus>();

            HandleCommandObject(type => services.RegisterSingleton(type),
                assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray());

            return services;
        }

        public static void UseEventBus(this IContainer app, params Assembly[] assemblies)
        {
            if (app.Resolve<IEventBus>() is not { } messenger)
            {
                throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
            }

            HandleCommandObject(type => messenger.Subscribe(app.Resolve(type)),
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
                                && t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                BindingFlags.NonPublic).Any(m =>
                                    m.GetCustomAttributes<EventHandlerAttribute>().Any()));

                foreach (var type in types)
                {
                    handleRecipient(type);
                }
            }
        }
    }
}