using System;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public interface IEventBus
    {
        void Subscribe(object recipient);
        void Subscribe<TCommand>(object recipient, Action<TCommand> action) where TCommand : Command;
        void Subscribe<TCommand>(object recipient, Func<TCommand, Task> asyncAction) where TCommand : Command;
        void Unsubscribe(object recipient);
        void Unsubscribe<TCommand>(object recipient, Action<TCommand> action = null) where TCommand : Command;
        void Unsubscribe<TCommand>(object recipient, Func<TCommand, Task> asyncAction = null) where TCommand : Command;
        void Publish<TCommand>(object sender, TCommand command) where TCommand : Command;
        Task PublishAsync<TCommand>(object sender, TCommand command) where TCommand : Command;
    }
}