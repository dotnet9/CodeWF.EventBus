using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public interface IEventBus
    {
        void Subscribe<T>() where T : class;
        void Subscribe(Type type);
        void Subscribe(object recipient);
        void Subscribe<TCommand>(Action<TCommand> action) where TCommand : Command;
        void Subscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command;
        void Subscribe(Assembly[] assemblies);

        void Unsubscribe<T>() where T : class;
        void Unsubscribe(object recipient);
        void Unsubscribe<TCommand>(Action<TCommand> action) where TCommand : Command;
        void Unsubscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command;

        void Publish<TCommand>(TCommand command) where TCommand : Command;
        TResponse Query<TResponse>(Query<TResponse> query);
        Task PublishAsync<TCommand>(TCommand command) where TCommand : Command;
        Task<TResponse> QueryAsync<TResponse>(Query<TResponse> query);

        void RegisterServiceHandlerAction(Action<Type, Action<object>> serviceHandlerAction);
    }
}