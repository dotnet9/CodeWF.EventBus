using System;
using System.Linq;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public partial class EventBus
    {
        private static Task InvokeHandler(Delegate handler, object command)
        {
            if (handler.Method.ReturnType == typeof(Task))
            {
                return (Task)handler.DynamicInvoke(command);
            }

            handler.DynamicInvoke(command);
            return null;
        }

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
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandType = command.GetType();
            var handlers = GetSubscriptionSnapshot(commandType);
            foreach (var handler in handlers)
            {
                var task = InvokeHandler(handler.Action, command);
                if (task != null)
                {
                    await task;
                }
            }

            var autoHandlers = GetAutoHandlerSnapshot(commandType);
            if (autoHandlers.Length > 0)
            {
                ThrowIfAutoHandlersAreNotConfigured();
                foreach (var handler in autoHandlers)
                {
                    var methodInfo = handler.Method;
                    Task task = null;
                    _serviceHandlerAction(handler.RecipientType, recipient =>
                    {
                        var delegateType = methodInfo.ReturnType == typeof(Task)
                            ? typeof(Func<,>).MakeGenericType(commandType, typeof(Task))
                            : typeof(Action<>).MakeGenericType(commandType);
                        var delegateInstance = Delegate.CreateDelegate(delegateType, recipient, methodInfo);
                        task = InvokeHandler(delegateInstance, command);
                    });

                    if (task != null)
                    {
                        await task;
                    }
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
