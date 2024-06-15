using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeWF.EventBus
{
    public class EventBus : IEventBus
    {
        public static readonly EventBus Default = new EventBus();

        private readonly ConcurrentDictionary<Type, List<WeakActionAndToken>> _subscriptions =
            new ConcurrentDictionary<Type, List<WeakActionAndToken>>();

        public void Subscribe(object recipient)
        {
            var methods = recipient.GetType().GetMethods();
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
                Subscribe(recipient, commandType, delegateInstance, eventHandlerAttr.Order);
            }
        }

        public void Subscribe<TCommand>(object recipient, Action<TCommand> action)
            where TCommand : Command
        {
            Subscribe(recipient, typeof(ICommand), action);
        }


        public void Subscribe<TCommand>(object recipient, Func<TCommand, Task> asyncAction) where TCommand : Command
        {
            Subscribe(recipient, typeof(ICommand), asyncAction);
        }

        private void Subscribe(object recipient, Type commandType, Delegate action, int order = 0)
        {
            var subscriptions = _subscriptions.GetOrAdd(commandType, _ => new List<WeakActionAndToken>());
            subscriptions.Add(new WeakActionAndToken()
                { Recipient = recipient, Action = action, Order = order });
        }

        public void Unsubscribe(object recipient)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.RemoveAll(item => item.Action.Target == recipient);
            }
        }

        public void Unsubscribe<TCommand>(object recipient, Action<TCommand> action = null) where TCommand : Command
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.RemoveAll(item =>
                    item.Action.Target == recipient && item.Action.Method.Name == action?.Method.Name);
            }
        }

        public void Unsubscribe<TCommand>(object recipient, Func<TCommand, Task> asyncAction = null)
            where TCommand : Command
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.RemoveAll(item =>
                    item.Action.Target == recipient && item.Action.Method.Name == asyncAction?.Method.Name);
            }
        }

        public void Publish<TCommand>(object sender, TCommand command) where TCommand : Command
        {
            // Note: This method does not wait for asynchronous subscription completion
            PublishAsync(sender, command).GetAwaiter()
                .GetResult(); // This may cause deadlocks and should be used with caution in UI threads or synchronization contexts
        }

        public async Task PublishAsync<TCommand>(object sender, TCommand command) where TCommand : Command
        {
            if (_subscriptions.TryGetValue(typeof(TCommand), out var handlers))
            {
                foreach (var handler in handlers.OrderBy(item => item.Order))
                {
                    if (handler.Action is Action<TCommand> syncAction)
                    {
                        syncAction.Invoke(command);
                    }
                    else if (handler.Action is Func<TCommand, Task> asyncAction)
                    {
                        await asyncAction.Invoke(command);
                    }
                }
            }
        }
    }
}