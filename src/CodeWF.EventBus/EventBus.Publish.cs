using System;
using System.Linq;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public partial class EventBus
    {
        public void Publish<TCommand>(TCommand command) where TCommand : Command
        {
            PublishAsync(command).GetAwaiter().GetResult();
        }

        public T Query<T>(Query<T> query)
        {
            Publish(query);
            return query.Result;
        }

        public async Task PublishAsync<TCommand>(TCommand command) where TCommand : Command
        {
            var commandType = command.GetType();
            if (_subscriptions.TryGetValue(commandType, out var handlers))
            {
                foreach (var handler in handlers.OrderBy(item => item.Order))
                {
                    if (handler.Action.Method.ReturnType == typeof(Task))
                    {
                        var task = (Task)handler.Action.DynamicInvoke(command);
                        await task;
                    }
                    else
                    {
                        handler.Action.DynamicInvoke(command);
                    }
                }
            }

            if (_autoHandlers.TryGetValue(commandType, out var autoHandlers))
            {
                foreach (var handler in autoHandlers.OrderBy(item => item.Order))
                {
                    var methodInfo = handler.Method;
                    _serviceHandlerAction(handler.RecipientType, recipient =>
                    {
                        var delegateType = methodInfo.ReturnType == typeof(Task)
                            ? typeof(Func<,>).MakeGenericType(commandType, typeof(Task))
                            : typeof(Action<>).MakeGenericType(commandType);
                        var delegateInstance = Delegate.CreateDelegate(delegateType, recipient, methodInfo);
                        if (handler.Method.ReturnType == typeof(Task))
                        {
                            ((Task)delegateInstance.DynamicInvoke(command)).GetAwaiter().GetResult();
                        }
                        else
                        {
                            delegateInstance.DynamicInvoke(command);
                        }
                    });
                }
            }
        }

        public async Task<T> QueryAsync<T>(Query<T> query)
        {
            await PublishAsync(query);
            return query.Result;
        }
    }
}