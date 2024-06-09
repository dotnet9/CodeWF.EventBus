using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeWF.EventBus
{
    public class Messenger : IMessenger
    {
        /// <summary>
        /// 提供Messenger类的默认实例
        /// </summary>
        public static readonly Messenger Default = new Messenger();

        /// <summary>
        /// 用于同步注册订阅者的锁对象
        /// </summary>
        private readonly object _registerLock = new object();

        /// <summary>  
        /// 存储订阅者和它们订阅的消息类型以及对应的操作（WeakActionAndToken列表）的字典。  
        /// 键是消息类型的Type对象，值是订阅该类型消息的订阅者列表（WeakActionAndToken的列表）。  
        /// </summary>  
        private Dictionary<Type, List<WeakActionAndToken>> _recipientsOfSubclassesAction;

        /// <summary>  
        /// 订阅所有类型的消息（无特定类型）。  
        /// 注意：消息处理方法需要添加EventHandlerAttribute特性，并且参数为消息类型
        /// </summary>  
        /// <param name="recipient">订阅者对象</param>
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
                var actionType = typeof(Action<>).MakeGenericType(messageType);
                var delegateInstance = Delegate.CreateDelegate(actionType, recipient, methodInfo);

                lock (_registerLock)
                {
                    if (_recipientsOfSubclassesAction == null)
                        _recipientsOfSubclassesAction =
                            new Dictionary<Type, List<WeakActionAndToken>>(); // 注意这里存储的是object  

                    if (!_recipientsOfSubclassesAction.TryGetValue(messageType, out var actionList))
                    {
                        actionList = new List<WeakActionAndToken>();
                        _recipientsOfSubclassesAction.Add(messageType, actionList);
                    }

                    var item = new WeakActionAndToken()
                        { Recipient = recipient, Action = delegateInstance, Order = eventHandlerAttr.Order };

                    actionList.Add(item);
                }
            }
        }

        /// <summary>  
        /// 订阅特定类型的消息
        /// </summary>  
        /// <typeparam name="TMessage">要订阅的消息类型，必须是Message类的派生类</typeparam>  
        /// <param name="recipient">订阅者对象</param>  
        /// <param name="action">当接收到消息时调用的操作</param>  
        public void Subscribe<TMessage>(object recipient, Action<TMessage> action)
            where TMessage : Message
        {
            lock (_registerLock)
            {
                var messageType = typeof(TMessage);

                if (_recipientsOfSubclassesAction == null)
                    _recipientsOfSubclassesAction = new Dictionary<Type, List<WeakActionAndToken>>();


                if (!_recipientsOfSubclassesAction.TryGetValue(messageType, out var actionList))
                {
                    actionList = new List<WeakActionAndToken>();
                    _recipientsOfSubclassesAction.Add(messageType, actionList);
                }

                var item = new WeakActionAndToken()
                    { Recipient = recipient, Action = action };

                actionList.Add(item);
            }
        }

        /// <summary>  
        /// 取消订阅指定对象上的所有事件处理函数  
        /// </summary>  
        /// <param name="recipient">订阅者对象。</param>  
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
                if (parameters.Length != 1 || !typeof(Message).IsAssignableFrom(parameters[0].ParameterType))
                {
                    continue;
                }

                var messageType = parameters[0].ParameterType;

                if (_recipientsOfSubclassesAction == null ||
                    !_recipientsOfSubclassesAction.TryGetValue(messageType, out var actionList)) continue;
                for (var i = actionList.Count - 1; i >= 0; i--)
                {
                    var item = actionList[i];

                    if (item != null && item.Recipient == recipient)
                    {
                        actionList.RemoveAt(i);
                    }
                }

                actionList.Clear();
                _recipientsOfSubclassesAction.Remove(messageType);
            }
        }

        /// <summary>  
        /// 取消订阅指定对象上的特定事件处理函数  
        /// </summary>  
        /// <typeparam name="TMessage">要取消订阅的消息类型，必须是Message类的派生类。</typeparam>  
        /// <param name="recipient">订阅者对象。</param>  
        /// <param name="action">要取消订阅的操作，如果为null，则取消该订阅者所有对TMessage类型的订阅。</param> 
        public void Unsubscribe<TMessage>(object recipient, Action<TMessage> action) where TMessage : Message
        {
            var messageType = typeof(TMessage);

            if (recipient == null
                || _recipientsOfSubclassesAction == null
                || _recipientsOfSubclassesAction.Count == 0
                || !_recipientsOfSubclassesAction.TryGetValue(messageType, out var actionList))
                return;

            for (var i = actionList.Count - 1; i >= 0; i--)
            {
                var item = actionList[i];
                var pastAction = item.Action;

                if (pastAction != null
                    && recipient == pastAction.Target
                    && (action == null || action.Method.Name == pastAction.Method.Name))
                    actionList.Remove(item);
            }
        }

        /// <summary>  
        /// 发布消息  
        /// </summary>  
        /// <typeparam name="TMessage">要发布的消息类型，必须是Message类的派生类</typeparam>  
        /// <param name="sender">消息的发送者</param>  
        /// <param name="message">要发布的消息</param> 
        public void Publish<TMessage>(object sender, TMessage message) where TMessage : Message
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

        /// <summary>  
        /// 发送消息到订阅者列表  
        /// </summary>  
        /// <typeparam name="TMessage">要发送的消息类型，必须是Message类的派生类</typeparam>  
        /// <param name="message">要发送的消息</param>  
        /// <param name="weakActionsAndTokens">订阅者列表，包含WeakAction和Token的配对</param>  
        private void SendToList<TMessage>(TMessage message, IEnumerable<WeakActionAndToken> weakActionsAndTokens)
            where TMessage : Message
        {
            var list = weakActionsAndTokens.ToList();
            var listClone = list.Take(list.Count()).ToList();

            foreach (var item in listClone)
                if (item.Action != null && item.Action.Target != null)
                    item.ExecuteWithObject(message);
        }
    }
}