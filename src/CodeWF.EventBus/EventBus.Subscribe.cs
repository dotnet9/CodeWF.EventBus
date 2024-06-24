using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public partial class EventBus
    {
        public void Subscribe<T>() where T : class
        {
            Subscribe(typeof(T));
        }

        public void Subscribe(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Subscribe(type, null, methods);
        }

        public void Subscribe(object recipient)
        {
            var recipientType = recipient.GetType();
            var methods = recipientType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Subscribe(recipientType, recipient, methods);
        }

        public void Subscribe<TCommand>(Action<TCommand> action)
            where TCommand : Command
        {
            Subscribe(typeof(TCommand), null, action);
        }

        public void Subscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command
        {
            Subscribe(typeof(TCommand), null, asyncAction);
        }

        public void Subscribe(Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass
                                && !t.IsAbstract
                                && t.GetCustomAttributes<EventAttribute>().Any()
                                && t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Any(m =>
                                        m.GetCustomAttributes<EventHandlerAttribute>().Any()));

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(m =>
                            m.GetCustomAttributes<EventHandlerAttribute>().Any());
                    foreach (var method in methods)
                    {
                        var eventHandler = method.GetCustomAttributes<EventHandlerAttribute>().First();
                        var parameters = method.GetParameters();
                        if (parameters.Length != 1 || !typeof(Command).IsAssignableFrom(parameters[0].ParameterType))
                        {
                            continue;
                        }

                        var commandType = parameters[0].ParameterType;

                        var subscriptions = _autoHandlers.GetOrAdd(commandType, _ => new List<WeakMethod>());
                        subscriptions.Add(new WeakMethod()
                            { RecipientType = type, Method = method, Order = eventHandler.Order });
                    }
                }
            }
        }

        private void Subscribe(Type recipientType, object recipient, MethodInfo[] methods)
        {
            foreach (var methodInfo in methods)
            {
                var eventHandlerAttr = methodInfo.GetCustomAttribute<EventHandlerAttribute>();
                if (eventHandlerAttr == null)
                {
                    continue;
                }

                var parameters = methodInfo.GetParameters();
                if (parameters.Length != 1 || !typeof(Command).IsAssignableFrom(parameters[0].ParameterType))
                {
                    continue;
                }

                if (methodInfo.ReturnType != typeof(void) && methodInfo.ReturnType != typeof(Task))
                {
                    continue;
                }

                var commandType = parameters[0].ParameterType;
                var delegateType = methodInfo.ReturnType == typeof(Task)
                    ? typeof(Func<,>).MakeGenericType(commandType, typeof(Task))
                    : typeof(Action<>).MakeGenericType(commandType);
                var delegateInstance = Delegate.CreateDelegate(delegateType, recipient, methodInfo);
                Subscribe(commandType, recipientType, delegateInstance, eventHandlerAttr.Order);
            }
        }

        private void Subscribe(Type commandType, Type recipientType, Delegate action, int order = 0)
        {
            var subscriptions = _subscriptions.GetOrAdd(commandType, _ => new List<WeakActionAndToken>());
            subscriptions.Add(new WeakActionAndToken()
                { RecipientType = recipientType, Action = action, Order = order });
        }
    }
}