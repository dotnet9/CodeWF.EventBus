using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeWF.EventBus
{
    public class EventBus : IEventBus
    {
        public static readonly EventBus Default = new EventBus();

        private readonly object _registerLock = new object();

        private Dictionary<Type, List<WeakActionAndToken>> _recipientsOfSubclassesAction;

        public void Subscribe(object recipient)
        {
            var recipientType = recipient.GetType();

            foreach (var methodInfo in recipientType.GetMethods(BindingFlags.Instance | BindingFlags.Public |
                                                                BindingFlags.NonPublic))
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

                var messageType = parameters[0].ParameterType;
                var actionType = typeof(Action<>).MakeGenericType(messageType);
                var delegateInstance = Delegate.CreateDelegate(actionType, recipient, methodInfo);

                Subscribe(messageType, new WeakActionAndToken()
                    { Recipient = recipient, Action = delegateInstance, Order = eventHandlerAttr.Order });
            }
        }

        public void Subscribe<TMessage>(object recipient, Action<TMessage> action)
            where TMessage : Command
        {
            var messageType = typeof(TMessage);
            Subscribe(messageType, new WeakActionAndToken()
                { Recipient = recipient, Action = action });
        }

        private void Subscribe(Type messageType, WeakActionAndToken actionInfo)
        {
            lock (_registerLock)
            {
                if (_recipientsOfSubclassesAction == null)
                {
                    _recipientsOfSubclassesAction = new Dictionary<Type, List<WeakActionAndToken>>();
                }

                if (!_recipientsOfSubclassesAction.TryGetValue(messageType, out var actionList))
                {
                    actionList = new List<WeakActionAndToken>();
                    _recipientsOfSubclassesAction.Add(messageType, actionList);
                }

                actionList.Add(actionInfo);
            }
        }

        public void Unsubscribe(object recipient)
        {
            var recipientType = recipient.GetType();

            foreach (var methodInfo in recipientType.GetMethods(BindingFlags.Instance | BindingFlags.Public |
                                                                BindingFlags.NonPublic))
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

                var messageType = parameters[0].ParameterType;

                if (_recipientsOfSubclassesAction == null ||
                    !_recipientsOfSubclassesAction.TryGetValue(messageType, out var actionList)) continue;


                actionList.RemoveAll(item =>
                    item.Action != null && item.Action.Target == recipient);
            }
        }

        public void Unsubscribe<TMessage>(object recipient, Action<TMessage> action = null) where TMessage : Command
        {
            var messageType = typeof(TMessage);

            if (recipient == null
                || _recipientsOfSubclassesAction == null
                || _recipientsOfSubclassesAction.Count == 0
                || !_recipientsOfSubclassesAction.TryGetValue(messageType, out var actionList))
                return;

            actionList.RemoveAll(item =>
                item.Action != null && item.Action.Target == recipient &&
                (action == null || item.Action.Method.Name == action.Method.Name));
        }

        public void Publish<TMessage>(object sender, TMessage message) where TMessage : Command
        {
            var messageType = typeof(TMessage);

            if (_recipientsOfSubclassesAction == null) return;

            var listClone = _recipientsOfSubclassesAction.Keys.Take(_recipientsOfSubclassesAction.Count)
                .ToList();

            foreach (var type in listClone)
            {
                List<WeakActionAndToken> list = null;

                if (messageType == type || messageType.IsSubclassOf(type) || type.IsAssignableFrom(messageType))
                    list = _recipientsOfSubclassesAction[type]
                        .Take(_recipientsOfSubclassesAction[type].Count)
                        .OrderBy(action => action.Order)
                        .ToList();

                if (list != null && list.Count > 0) SendToList(message, list);
            }
        }

        private void SendToList<TMessage>(TMessage message, IEnumerable<WeakActionAndToken> weakActionsAndTokens)
            where TMessage : Command
        {
            var list = weakActionsAndTokens.ToList();
            var listClone = list.Take(list.Count()).ToList();

            foreach (var item in listClone)
                if (item.Action != null && item.Action.Target != null)
                    item.ExecuteWithObject(message);
        }
    }
}