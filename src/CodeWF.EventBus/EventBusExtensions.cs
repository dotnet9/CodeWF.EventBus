using System;
using System.Linq;
using System.Reflection;

namespace CodeWF.EventBus
{
    public static class EventBusExtensions
    {
        public static void HandleEventObject(Action<Type> handleRecipient, BindingFlags findHandlerMethodBindingFlags,
            Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass
                                && !t.IsAbstract
                                && t.GetCustomAttributes<EventAttribute>().Any()
                                && t.GetMethods(findHandlerMethodBindingFlags)
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