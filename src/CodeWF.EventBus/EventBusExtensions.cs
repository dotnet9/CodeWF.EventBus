using System;
using System.Linq;
using System.Reflection;

namespace CodeWF.EventBus
{
    public static class EventBusExtensions
    {
        public static void AddEventBus(Action<Type, Type> addSingleton1,
            Action<Type> addSingleton2, params Assembly[] assemblies)
        {
            addSingleton1(typeof(IEventBus), typeof(EventBus));
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