using CodeWF.EventBus;
using System;
using System.Linq;
using System.Reflection;

namespace CodeWF.IOC.EventBus
{
    /// <summary>
    /// 通用 IOC 场景下的事件总线扩展。
    /// </summary>
    public static class EventBusExtensions
    {
        private static Assembly[] GetAssemblies(Assembly[] assemblies)
        {
            var requestedAssemblies = assemblies ?? Array.Empty<Assembly>();
            if (requestedAssemblies.Any(assembly => assembly == null))
            {
                throw new ArgumentException("The assemblies collection cannot contain null entries.", nameof(assemblies));
            }

            var entryAssembly = Assembly.GetEntryAssembly();
            var assembliesToScan = requestedAssemblies.Length > 0
                ? requestedAssemblies.Concat(entryAssembly == null ? Array.Empty<Assembly>() : new[] { entryAssembly })
                : entryAssembly == null
                    ? AppDomain.CurrentDomain.GetAssemblies()
                    : new[] { entryAssembly };

            return assembliesToScan
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// 注册事件总线以及自动发现的实例处理器类型。
        /// </summary>
        public static void AddEventBus(Action<Type, Type> addSingleton1,
            Action<Type> addScoped2, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(addSingleton1);
            ArgumentNullException.ThrowIfNull(addScoped2);
            addSingleton1(typeof(IEventBus), typeof(CodeWF.EventBus.EventBus));

            var allAssemblies = GetAssemblies(assemblies);

            CodeWF.EventBus.EventBusExtensions.HandleEventObject(addScoped2,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                allAssemblies);
        }

        /// <summary>
        /// 启动事件总线，并提供按类型解析实例处理器的方式。
        /// </summary>
        public static void UseEventBus(Func<Type, object> resolveAction, params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(resolveAction);
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
