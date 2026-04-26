using System;
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
