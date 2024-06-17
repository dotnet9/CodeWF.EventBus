using System;
using System.Collections.Generic;
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
            Subscribe(null, methods);
        }

        public void Subscribe(object recipient)
        {
            var methods = recipient.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Subscribe(recipient, methods);
        }


        public void Subscribe<TCommand>(Action<TCommand> action)
            where TCommand : Command
        {
            Subscribe(typeof(TCommand), action);
        }

        public void Subscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command
        {
            Subscribe(typeof(TCommand), asyncAction);
        }

        private void Subscribe(object recipient, MethodInfo[] methods)
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
                Subscribe(commandType, delegateInstance, eventHandlerAttr.Order);
            }
        }

        private void Subscribe(Type commandType, Delegate action, int order = 0)
        {
            var subscriptions = _subscriptions.GetOrAdd(commandType, _ => new List<WeakActionAndToken>());
            subscriptions.Add(new WeakActionAndToken()
                { Action = action, Order = order });
        }
    }
}