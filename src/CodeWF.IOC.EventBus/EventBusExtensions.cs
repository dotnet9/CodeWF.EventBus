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
            HandleCommandObject(addSingleton2, assemblies);
        }

        public static void UseEventBus(Func<Type, object> resolveAction, params Assembly[] assemblies)
        {
            if (!(resolveAction(typeof(IEventBus)) is IEventBus messenger))
            {
                throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
            }

            HandleCommandObject(type => messenger.Subscribe(resolveAction(type)),
                assemblies);
        }

        private static void HandleCommandObject(Action<Type> handleRecipient, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies.Concat(new[] { Assembly.GetCallingAssembly() }))
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