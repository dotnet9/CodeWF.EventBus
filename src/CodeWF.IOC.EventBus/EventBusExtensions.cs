using CodeWF.EventBus;
using System;
using System.Linq;
using System.Reflection;

namespace CodeWF.IOC.EventBus
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

        public static void AddEventBus(Action<Type, Type> addSingleton1,
            Action<Type> addScoped2, params Assembly[] assemblies)
        {
            addSingleton1(typeof(IEventBus), typeof(CodeWF.EventBus.EventBus));

            var allAssemblies = GetAssemblies(assemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(addScoped2,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                allAssemblies);
        }

        public static void UseEventBus(Func<Type, object> resolveAction, params Assembly[] assemblies)
        {
            if (resolveAction(typeof(IEventBus)) is not IEventBus messenger)
            {
                throw new InvalidOperationException("Please call AddEventBus before calling UseEventBus");
            }

            var allAssemblies = GetAssemblies(assemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(messenger.Subscribe,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, allAssemblies);
            messenger.RegisterServiceHandlerAction((type, action) =>
            {
                var obj = resolveAction(type);
                action(obj);
            });
            messenger.Subscribe(allAssemblies);
        }
    }
}
