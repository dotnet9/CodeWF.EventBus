using System;
using System.Linq;
using CodeWF.EventBus;
using DryIoc;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;

namespace CodeWF.DryIoc.EventBus
{
    public static class EventBusExtensions
    {
        public static IContainerRegistry AddEventBus(this IContainerRegistry services, params Assembly[] assemblies)
        {
            services.RegisterSingleton<IEventBus, CodeWF.EventBus.EventBus>();

            var allAssemblies = assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray();

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => services.RegisterScoped(type),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                allAssemblies);

            return services;
        }

        public static void UseEventBus(this IContainer app, params Assembly[] assemblies)
        {
            if (app.Resolve<IEventBus>() is not { } messenger)
            {
                throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
            }

            var allAssemblies = assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray();

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => messenger.Subscribe(type),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);
            messenger.Subscribe(allAssemblies);
            messenger.RegisterServiceHandlerAction((type, action) =>
            {
                using var scope = app.CreateScope();
                var obj = scope.ServiceProvider.GetRequiredService(type);
                action(obj);
            });
        }
    }
}