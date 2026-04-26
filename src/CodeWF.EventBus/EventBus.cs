using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeWF.EventBus
{
    /// <summary>
    /// 默认事件总线实现。
    /// </summary>
    public partial class EventBus : IEventBus
    {
        /// <summary>
        /// 全局默认实例，适合无 IOC 场景下直接使用。
        /// </summary>
        public static readonly EventBus Default = new EventBus();

        private readonly ConcurrentDictionary<Type, List<SubscriptionEntry>> _subscriptions =
            new ConcurrentDictionary<Type, List<SubscriptionEntry>>();

        private readonly ConcurrentDictionary<Type, List<DiscoveredHandlerMethod>>
            _autoHandlers = new ConcurrentDictionary<Type, List<DiscoveredHandlerMethod>>();

        // _subscriptions 与 _autoHandlers 的值类型都是 List，
        // 因此除了 ConcurrentDictionary 之外仍需额外加锁保证修改安全。
        private readonly object _subscriptionsSync = new object();
        private readonly object _autoHandlersSync = new object();

        private Action<Type, Action<object>> _serviceHandlerAction;

        private bool IsTheSameMethod(MethodInfo method1, MethodInfo method2)
        {
            return method1.DeclaringType == method2.DeclaringType &&
                   GetMethodSignature(method1).Equals(GetMethodSignature(method2));
        }

        private string GetMethodSignature(MethodInfo methodInfo)
        {
            var parameters = string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.FullName));
            return methodInfo.Name + "(" + parameters + ")";
        }

        private bool IsTheSameDelegate(Delegate left, Delegate right)
        {
            return ReferenceEquals(left.Target, right.Target) &&
                   IsTheSameMethod(left.Method, right.Method);
        }

        private SubscriptionEntry[] GetSubscriptionSnapshot(Type commandType)
        {
            lock (_subscriptionsSync)
            {
                if (!_subscriptions.TryGetValue(commandType, out var handlers) || handlers.Count == 0)
                {
                    return Array.Empty<SubscriptionEntry>();
                }

                // 发布时使用快照，避免一边发布一边订阅/取消订阅导致枚举异常。
                return handlers
                    .OrderBy(item => item.Order)
                    .ToArray();
            }
        }

        private DiscoveredHandlerMethod[] GetAutoHandlerSnapshot(Type commandType)
        {
            lock (_autoHandlersSync)
            {
                if (!_autoHandlers.TryGetValue(commandType, out var handlers) || handlers.Count == 0)
                {
                    return Array.Empty<DiscoveredHandlerMethod>();
                }

                // 自动处理器同样按快照发布，保证读取阶段稳定。
                return handlers
                    .OrderBy(item => item.Order)
                    .ToArray();
            }
        }

        private void ThrowIfAutoHandlersAreNotConfigured()
        {
            if (_serviceHandlerAction == null)
            {
                throw new InvalidOperationException(
                    "Instance event handlers discovered from assemblies require a service resolver. " +
                    "Call RegisterServiceHandlerAction or use one of the IOC integration packages before publishing.");
            }
        }

        /// <summary>
        /// 注册实例处理器的服务解析回调。
        /// </summary>
        /// <param name="serviceHandlerAction">服务解析与执行回调。</param>
        public void RegisterServiceHandlerAction(Action<Type, Action<object>> serviceHandlerAction)
        {
            if (serviceHandlerAction == null)
            {
                throw new ArgumentNullException(nameof(serviceHandlerAction));
            }

            _serviceHandlerAction = serviceHandlerAction;
        }
    }
}
