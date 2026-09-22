using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeWF.EventBus
{
    /// <summary>
    /// 主库中的程序集扫描辅助方法。
    /// 供不同 IOC 集成层复用。
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>
        /// 扫描程序集，找出包含事件处理方法的类型并交给调用方处理。
        /// </summary>
        /// <param name="handleRecipient">扫描命中后的处理动作。</param>
        /// <param name="findHandlerMethodBindingFlags">查找方法时使用的绑定标志，决定本次扫描命中哪些方法。</param>
        /// <param name="assemblies">待扫描程序集。</param>
        public static void HandleEventObject(Action<Type> handleRecipient, BindingFlags findHandlerMethodBindingFlags,
            Assembly[] assemblies)
        {
            if (handleRecipient == null)
            {
                throw new ArgumentNullException(nameof(handleRecipient));
            }

            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            foreach (var assembly in assemblies)
            {
                var types = GetLoadableTypes(assembly)
                    .Where(t => t.IsClass
                                && !t.IsAbstract
                                && t.GetCustomAttributes<EventAttribute>().Any()
                                && t.GetMethods(findHandlerMethodBindingFlags)
                                    .Any(m =>
                                        m.GetCustomAttributes<EventHandlerAttribute>().Any() &&
                                        EventBus.IsValidHandlerMethod(m)));

                foreach (var type in types)
                {
                    handleRecipient(type);
                }
            }
        }

        internal static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }
    }
}
