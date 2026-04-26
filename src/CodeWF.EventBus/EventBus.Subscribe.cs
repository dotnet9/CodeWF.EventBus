using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public partial class EventBus
    {
        /// <summary>
        /// 扫描指定类型，并订阅其中标记了 <see cref="EventHandlerAttribute"/> 的 public/private static 处理方法。
        /// </summary>
        public void Subscribe<T>() where T : class
        {
            Subscribe(typeof(T));
        }

        /// <summary>
        /// 扫描指定类型，并订阅其中标记了 <see cref="EventHandlerAttribute"/> 的 public/private static 处理方法。
        /// </summary>
        public void Subscribe(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Subscribe(type, null, methods);
        }

        /// <summary>
        /// 订阅指定实例上的处理方法。
        /// </summary>
        public void Subscribe(object recipient)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            var recipientType = recipient.GetType();
            var methods = recipientType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Subscribe(recipientType, recipient, methods);
        }

        /// <summary>
        /// 直接订阅同步命令处理委托。
        /// </summary>
        public void Subscribe<TCommand>(Action<TCommand> action)
            where TCommand : Command
        {
            Subscribe(typeof(TCommand), null, action);
        }

        /// <summary>
        /// 直接订阅异步命令处理委托。
        /// </summary>
        public void Subscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command
        {
            Subscribe(typeof(TCommand), null, asyncAction);
        }

        /// <summary>
        /// 扫描程序集，登记带 <see cref="EventAttribute"/> 的实例处理器。
        /// 注意这里只做登记，真正发布时仍需要先提供服务解析回调。
        /// </summary>
        public void Subscribe(Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass
                                && !t.IsAbstract
                                && t.GetCustomAttributes<EventAttribute>().Any()
                                && t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Any(m =>
                                        m.GetCustomAttributes<EventHandlerAttribute>().Any()));

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(m =>
                            m.GetCustomAttributes<EventHandlerAttribute>().Any());
                    foreach (var method in methods)
                    {
                        var eventHandler = method.GetCustomAttributes<EventHandlerAttribute>().First();
                        var parameters = method.GetParameters();
                        if (parameters.Length != 1 || !typeof(Command).IsAssignableFrom(parameters[0].ParameterType))
                        {
                            continue;
                        }

                        var commandType = parameters[0].ParameterType;
                        lock (_autoHandlersSync)
                        {
                            var subscriptions = _autoHandlers.GetOrAdd(commandType, _ => new List<DiscoveredHandlerMethod>());
                            // 同一个类型上的同一个方法只登记一次，避免 UseEventBus 重复调用后重复执行。
                            if (subscriptions.Any(item =>
                                    item.RecipientType == type &&
                                    IsTheSameMethod(item.Method, method)))
                            {
                                continue;
                            }

                            subscriptions.Add(new DiscoveredHandlerMethod()
                                { RecipientType = type, Method = method, Order = eventHandler.Order });
                        }
                    }
                }
            }
        }

        private void Subscribe(Type recipientType, object recipient, MethodInfo[] methods)
        {
            foreach (var methodInfo in methods)
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

                if (methodInfo.ReturnType != typeof(void) && methodInfo.ReturnType != typeof(Task))
                {
                    continue;
                }

                var commandType = parameters[0].ParameterType;
                var delegateType = methodInfo.ReturnType == typeof(Task)
                    ? typeof(Func<,>).MakeGenericType(commandType, typeof(Task))
                    : typeof(Action<>).MakeGenericType(commandType);
                var delegateInstance = Delegate.CreateDelegate(delegateType, recipient, methodInfo);
                Subscribe(commandType, recipientType, delegateInstance, eventHandlerAttr.Order);
            }
        }

        private void Subscribe(Type commandType, Type recipientType, Delegate action, int order = 0)
        {
            lock (_subscriptionsSync)
            {
                var subscriptions = _subscriptions.GetOrAdd(commandType, _ => new List<SubscriptionEntry>());
                // 同一个对象方法或同一个扫描命中的方法重复注册时自动去重。
                if (subscriptions.Any(item =>
                        item.RecipientType == recipientType &&
                        IsTheSameDelegate(item.Action, action)))
                {
                    return;
                }

                subscriptions.Add(new SubscriptionEntry()
                    { RecipientType = recipientType, Action = action, Order = order });
            }
        }
    }
}
