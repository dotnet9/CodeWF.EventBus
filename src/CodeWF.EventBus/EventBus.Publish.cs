using System;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    public partial class EventBus
    {
        /// <summary>
        /// 统一执行同步/异步处理器，调用方按返回值是否为空判断是否需要等待。
        /// </summary>
        private static Task InvokeHandler(Delegate handler, object command)
        {
            if (handler.Method.ReturnType == typeof(Task))
            {
                return (Task)handler.DynamicInvoke(command);
            }

            handler.DynamicInvoke(command);
            return null;
        }

        /// <summary>
        /// 同步发布命令。
        /// </summary>
        public void Publish<TCommand>(TCommand command) where TCommand : Command
        {
            PublishAsync(command).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 同步执行查询。
        /// </summary>
        public T Query<T>(Query<T> query)
        {
            Publish(query);
            return query.Result;
        }

        /// <summary>
        /// 异步发布命令。
        /// 先执行手动注册的处理器，再执行程序集扫描得到的实例处理器。
        /// </summary>
        public async Task PublishAsync<TCommand>(TCommand command) where TCommand : Command
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandType = command.GetType();
            var handlers = GetSubscriptionSnapshot(commandType);
            foreach (var handler in handlers)
            {
                var task = InvokeHandler(handler.Action, command);
                if (task != null)
                {
                    await task;
                }
            }

            var autoHandlers = GetAutoHandlerSnapshot(commandType);
            if (autoHandlers.Length > 0)
            {
                ThrowIfAutoHandlersAreNotConfigured();
                foreach (var handler in autoHandlers)
                {
                    var methodInfo = handler.Method;
                    Task task = null;
                    // 实例处理器的对象由 IOC 集成层提供，主库只负责“拿到对象后执行方法”。
                    _serviceHandlerAction(handler.RecipientType, recipient =>
                    {
                        var delegateType = methodInfo.ReturnType == typeof(Task)
                            ? typeof(Func<,>).MakeGenericType(commandType, typeof(Task))
                            : typeof(Action<>).MakeGenericType(commandType);
                        var delegateInstance = Delegate.CreateDelegate(delegateType, recipient, methodInfo);
                        task = InvokeHandler(delegateInstance, command);
                    });

                    if (task != null)
                    {
                        await task;
                    }
                }
            }
        }

        /// <summary>
        /// 异步执行查询。
        /// </summary>
        public async Task<T> QueryAsync<T>(Query<T> query)
        {
            await PublishAsync(query);
            return query.Result;
        }
    }
}
