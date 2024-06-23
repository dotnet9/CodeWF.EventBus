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
            if (_subscriptions.TryGetValue(command.GetType(), out var handlers))
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
        }

        public async Task<T> QueryAsync<T>(Query<T> query)
        {
            await PublishAsync(query);
            return query.Result;
        }
    }
}