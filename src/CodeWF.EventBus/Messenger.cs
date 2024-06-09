using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public class Messenger : IMessenger
    {
        /// <summary>
        /// 提供Messenger类的默认实例。
        /// </summary>
        public static readonly Messenger Default = new Messenger();

        /// <summary>
        /// 用于同步注册订阅者的锁对象。
        /// </summary>
        private readonly object _registerLock = new object();

        /// <summary>  
        /// 存储订阅者和它们订阅的消息类型以及对应的操作（WeakActionAndToken列表）的字典。  
        /// 键是消息类型的Type对象，值是订阅该类型消息的订阅者列表（WeakActionAndToken的列表）。  
        /// </summary>  
        private Dictionary<Type, List<WeakActionAndToken>> _recipientsOfSubclassesAction;

        /// <summary>  
        /// 根据给定的消息类型、方法信息和订阅者对象，创建一个能够处理该消息类型的Action委托。  
        /// </summary>  
        /// <param name="messageType">消息类型。</param>  
        /// <param name="methodInfo">包含消息处理逻辑的方法的MethodInfo。</param>  
        /// <param name="recipient">订阅者对象。</param>  
        /// <returns>一个Action<Message>委托，用于处理指定类型的消息。</returns>  
        /// <exception cref="ArgumentException">如果方法签名与事件处理程序期望的签名不匹配，则抛出此异常。</exception>
        private static Action<Message> CreateAction(Type messageType, MethodInfo methodInfo, object recipient)
        {
            var actionType = typeof(Action<>).MakeGenericType(messageType);
            var delegateInstance = Delegate.CreateDelegate(actionType, recipient, methodInfo);
            return (message) =>
            {
                if (messageType.IsInstanceOfType(message))
                {
                    delegateInstance.DynamicInvoke(message);
                }
            };
        }

        /// <summary>  
        /// 订阅指定的对象以接收消息。  
        /// </summary>  
        /// <param name="recipient">要订阅的对象。</param> 
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
                if (parameters.Length != 1 || !typeof(Message).IsAssignableFrom(parameters[0].ParameterType))
                {
                    continue;
                }

                var messageType = parameters[0].ParameterType;

                Subscribe(recipient, (Action<Message>)NonGenericAction);
                continue;

                void NonGenericAction(Message message)
                {
                    var typedMessage = (messageType.IsInstanceOfType(message) ? message : null);
                    if (typedMessage == null)
                    {
                        return;
                    }

                    var action = CreateAction(messageType, methodInfo, recipient);
                    action(typedMessage);
                }
            }
        }

        public void Subscribe<TMessage>(object recipient, Action<TMessage> action,
            ThreadOption threadOption = ThreadOption.PublisherThread,
            string tag = null)
            where TMessage : Message
        {
            lock (_registerLock)
            {
                var messageType = typeof(TMessage);

                if (_recipientsOfSubclassesAction == null)
                    _recipientsOfSubclassesAction = new Dictionary<Type, List<WeakActionAndToken>>();

                List<WeakActionAndToken> list;

                if (!_recipientsOfSubclassesAction.ContainsKey(messageType))
                {
                    list = new List<WeakActionAndToken>();
                    _recipientsOfSubclassesAction.Add(messageType, list);
                }
                else
                {
                    list = _recipientsOfSubclassesAction[messageType];
                }

                var item = new WeakActionAndToken()
                    { Recipient = recipient, ThreadOption = threadOption, Action = action, Tag = tag };

                list.Add(item);
            }
        }

        public void Unsubscribe<TMessage>(object recipient, Action<TMessage> action) where TMessage : Message
        {
            var messageType = typeof(TMessage);

            if (recipient == null || _recipientsOfSubclassesAction == null ||
                _recipientsOfSubclassesAction.Count == 0 || !_recipientsOfSubclassesAction.ContainsKey(messageType))
                return;

            var lstActions = _recipientsOfSubclassesAction[messageType];
            for (var i = lstActions.Count - 1; i >= 0; i--)
            {
                var item = lstActions[i];
                var pastAction = item.Action;

                if (pastAction != null
                    && recipient == pastAction.Target
                    && (action == null || action.Method.Name == pastAction.Method.Name))
                    lstActions.Remove(item);
            }
        }

        public void Publish<TMessage>(object sender, TMessage message, string tag = null) where TMessage : Message
        {
            var messageType = typeof(TMessage);

            if (_recipientsOfSubclassesAction != null)
            {
                var listClone = _recipientsOfSubclassesAction.Keys.Take(_recipientsOfSubclassesAction.Count)
                    .ToList();

                foreach (var type in listClone)
                {
                    List<WeakActionAndToken> list = null;

                    if (messageType == type || messageType.IsSubclassOf(type) || type.IsAssignableFrom(messageType))
                        list = _recipientsOfSubclassesAction[type]
                            .Take(_recipientsOfSubclassesAction[type].Count)
                            .Where(subscription => tag == null || subscription.Tag == tag).ToList();

                    if (list != null && list.Count > 0) SendToList(message, list);
                }
            }
        }

        private void SendToList<TMessage>(TMessage message, IEnumerable<WeakActionAndToken> weakActionsAndTokens)
            where TMessage : Message
        {
            var list = weakActionsAndTokens.ToList();
            var listClone = list.Take(list.Count()).ToList();

            foreach (var item in listClone)
                if (item.Action != null && item.Action.Target != null)
                    switch (item.ThreadOption)
                    {
                        case ThreadOption.BackgroundThread:
                            Task.Run(() => { item.ExecuteWithObject(message); });
                            break;
                        case ThreadOption.UiThread:
                            SynchronizationContext.Current?.Post(_ => { item.ExecuteWithObject(message); }, null);
                            break;
                        default:
                            item.ExecuteWithObject(message);
                            break;
                    }
        }
    }
}