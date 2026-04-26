using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeWF.EventBus
{
    public partial class EventBus : IEventBus
    {
        public static readonly EventBus Default = new EventBus();

        private readonly ConcurrentDictionary<Type, List<WeakActionAndToken>> _subscriptions =
            new ConcurrentDictionary<Type, List<WeakActionAndToken>>();

        private readonly ConcurrentDictionary<Type, List<WeakMethod>>
            _autoHandlers = new ConcurrentDictionary<Type, List<WeakMethod>>();

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

        private WeakActionAndToken[] GetSubscriptionSnapshot(Type commandType)
        {
            lock (_subscriptionsSync)
            {
                if (!_subscriptions.TryGetValue(commandType, out var handlers) || handlers.Count == 0)
                {
                    return Array.Empty<WeakActionAndToken>();
                }

                return handlers
                    .OrderBy(item => item.Order)
                    .ToArray();
            }
        }

        private WeakMethod[] GetAutoHandlerSnapshot(Type commandType)
        {
            lock (_autoHandlersSync)
            {
                if (!_autoHandlers.TryGetValue(commandType, out var handlers) || handlers.Count == 0)
                {
                    return Array.Empty<WeakMethod>();
                }

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
