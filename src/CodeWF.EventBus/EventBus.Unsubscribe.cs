using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public partial class EventBus
    {
        public void Unsubscribe<T>() where T : class
        {
            var methods = typeof(T)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.RemoveAll(item =>
                    item.Action.Target == null && methods.Any(method => method.Name == item.Action.Method.Name));
            }
        }

        public void Unsubscribe(object recipient)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.RemoveAll(item => item.Action.Target == recipient);
            }
        }

        public void Unsubscribe<TCommand>(Action<TCommand> action) where TCommand : Command
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.RemoveAll(item =>
                    item.Action == (Delegate)action);
            }
        }

        public void Unsubscribe<TCommand>(Func<TCommand, Task> asyncAction)
            where TCommand : Command
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Value.RemoveAll(item => item.Action == (Delegate)asyncAction);
            }
        }
    }
}