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

        public void RegisterServiceHandlerAction(Action<Type, Action<object>> serviceHandlerAction)
        {
            _serviceHandlerAction = serviceHandlerAction;
        }
    }
}