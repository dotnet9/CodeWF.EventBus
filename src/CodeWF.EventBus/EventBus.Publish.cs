using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
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
            object? result;
            try
            {
                result = handler.DynamicInvoke(command);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }

            if (handler.Method.ReturnType == typeof(Task))
            {
                return (Task?)result ?? Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private Task InvokeAutoHandler(DiscoveredHandlerMethod handler, Type commandType, object command)
        {
            Task? task = null;
            var methodInfo = handler.Method;
            var serviceHandlerAction = _serviceHandlerAction ?? throw new InvalidOperationException(
                "Instance event handlers discovered from assemblies require a service resolver. " +
                "Call RegisterServiceHandlerAction or use one of the IOC integration packages before publishing.");
            serviceHandlerAction(handler.RecipientType, recipient =>
            {
                if (recipient == null)
                {
                    throw new InvalidOperationException(
                        $"The service resolver returned null for event handler type '{handler.RecipientType.FullName}'.");
                }

                var delegateType = methodInfo.ReturnType == typeof(Task)
                    ? typeof(Func<,>).MakeGenericType(commandType, typeof(Task))
                    : typeof(Action<>).MakeGenericType(commandType);
                var delegateInstance = Delegate.CreateDelegate(delegateType, recipient, methodInfo);
                task = InvokeHandler(delegateInstance, command);
            });

            return task ?? Task.CompletedTask;
        }

        private void ThrowIfSynchronousPublishIsNotSupported(Type commandType)
        {
            var handlers = GetSubscriptionSnapshot(commandType);
            if (handlers.Any(handler => handler.Action.Method.ReturnType == typeof(Task)))
            {
                throw new InvalidOperationException(
                    "Synchronous publishing cannot execute asynchronous handlers. Use PublishAsync or QueryAsync instead.");
            }

            var autoHandlers = GetAutoHandlerSnapshot(commandType);
            if (autoHandlers.Any(handler => handler.Method.ReturnType == typeof(Task)))
            {
                throw new InvalidOperationException(
                    "Synchronous publishing cannot execute asynchronous handlers. Use PublishAsync or QueryAsync instead.");
            }
        }

        /// <summary>
        /// 同步发布命令。
        /// </summary>
        public void Publish<TCommand>(TCommand command) where TCommand : Command
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            ThrowIfSynchronousPublishIsNotSupported(command.GetType());
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
            var autoHandlers = GetAutoHandlerSnapshot(commandType);
            if (autoHandlers.Length > 0)
            {
                ThrowIfAutoHandlersAreNotConfigured();
            }

            var invocations = new List<(int Order, Func<Task> Invoke)>(handlers.Length + autoHandlers.Length);
            foreach (var handler in handlers)
            {
                var currentHandler = handler;
                invocations.Add((currentHandler.Order,
                    () => InvokeHandler(currentHandler.Action, command)));
            }

            foreach (var handler in autoHandlers)
            {
                var currentHandler = handler;
                invocations.Add((currentHandler.Order,
                    () => InvokeAutoHandler(currentHandler, commandType, command)));
            }

            foreach (var invocation in invocations.OrderBy(item => item.Order))
            {
                await invocation.Invoke();
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
