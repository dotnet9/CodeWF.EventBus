using System;
using System.Linq;
using System.Reflection;
using CodeWF.EventBus;

namespace CodeWF.IOC.EventBus
{
    public static class EventBusExtensions
    {
        public static void AddEventBus(Action<Type, Type> addSingleton1,
            Action<Type> addSingleton2, params Assembly[] assemblies)
        {
            addSingleton1(typeof(IEventBus), typeof(CodeWF.EventBus.EventBus));

            var allAssemblies = assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray();

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => addSingleton2(type),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                allAssemblies);
        }

        public static void UseEventBus(Func<Type, object> resolveAction, params Assembly[] assemblies)
        {
            if (!(resolveAction(typeof(IEventBus)) is IEventBus messenger))
            {
                throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
            }

            var allAssemblies = assemblies.Concat(new[] { Assembly.GetCallingAssembly() }).ToArray();

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(
                type => messenger.Subscribe(resolveAction(type)),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(type => messenger.Subscribe(type),
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);
        }
    }
}