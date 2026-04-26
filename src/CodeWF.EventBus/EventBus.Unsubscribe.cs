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

            lock (_subscriptionsSync)
            {
                foreach (var subscription in _subscriptions)
                {
                    subscription.Value.RemoveAll(item =>
                        item.Action.Target == null && methods.Any(method => IsTheSameMethod(item.Action.Method, method)));
                }
            }
        }

        public void Unsubscribe(object recipient)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            lock (_subscriptionsSync)
            {
                foreach (var subscription in _subscriptions)
                {
                    subscription.Value.RemoveAll(item => item.Action.Target == recipient);
                }
            }
        }

        public void Unsubscribe<TCommand>(Action<TCommand> action) where TCommand : Command
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_subscriptionsSync)
            {
                foreach (var subscription in _subscriptions)
                {
                    subscription.Value.RemoveAll(item => IsTheSameDelegate(item.Action, action));
                }
            }
        }

        public void Unsubscribe<TCommand>(Func<TCommand, Task> asyncAction)
            where TCommand : Command
        {
            if (asyncAction == null)
            {
                throw new ArgumentNullException(nameof(asyncAction));
            }

            lock (_subscriptionsSync)
            {
                foreach (var subscription in _subscriptions)
                {
                    subscription.Value.RemoveAll(item => IsTheSameDelegate(item.Action, asyncAction));
                }
            }
        }
    }
}
