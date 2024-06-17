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

        private string GetMethodSignature(MethodInfo methodInfo)
        {
            var parameters = string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.Name));
            return methodInfo.Name + "(" + parameters + ")";
        }
    }
}