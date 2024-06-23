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

        public async Task<T> QueryAsync<T>(Query<T> query)
        {
            await PublishAsync(query);
            return query.Result;
        }
    }
}