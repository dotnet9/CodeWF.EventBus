using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CodeWF.EventBus
{
    /// <summary>
    /// 事件总线抽象。
    /// 用于注册处理器、发布命令和执行查询。
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 扫描指定类型，并订阅其中标记了 <see cref="EventHandlerAttribute"/> 的 public/private static 处理方法。
        /// </summary>
        /// <typeparam name="T">待扫描的类型。</typeparam>
        void Subscribe<T>() where T : class;

        /// <summary>
        /// 扫描指定类型，并订阅其中标记了 <see cref="EventHandlerAttribute"/> 的 public/private static 处理方法。
        /// </summary>
        /// <param name="type">待扫描的类型。</param>
        void Subscribe(Type type);

        /// <summary>
        /// 订阅指定对象实例上的事件处理方法。
        /// </summary>
        /// <param name="recipient">处理器实例。会扫描其中标记了 <see cref="EventHandlerAttribute"/> 的 public/private instance 方法。</param>
        void Subscribe(object recipient);

        /// <summary>
        /// 直接订阅一个同步命令处理委托。
        /// </summary>
        /// <typeparam name="TCommand">命令类型。</typeparam>
        /// <param name="action">同步处理委托。</param>
        void Subscribe<TCommand>(Action<TCommand> action) where TCommand : Command;

        /// <summary>
        /// 直接订阅一个异步命令处理委托。
        /// </summary>
        /// <typeparam name="TCommand">命令类型。</typeparam>
        /// <param name="asyncAction">异步处理委托。</param>
        void Subscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command;

        /// <summary>
        /// 扫描程序集，将标注 <see cref="EventAttribute"/> 的实例处理器登记到自动处理列表。
        /// </summary>
        /// <param name="assemblies">需要扫描的程序集列表。</param>
        void Subscribe(Assembly[] assemblies);

        /// <summary>
        /// 取消扫描指定类型时注册的处理方法订阅。
        /// </summary>
        /// <typeparam name="T">待取消订阅的类型。</typeparam>
        void Unsubscribe<T>() where T : class;

        /// <summary>
        /// 取消指定实例上的所有事件处理方法订阅。
        /// </summary>
        /// <param name="recipient">处理器实例。</param>
        void Unsubscribe(object recipient);

        /// <summary>
        /// 取消指定同步委托的订阅。
        /// </summary>
        /// <typeparam name="TCommand">命令类型。</typeparam>
        /// <param name="action">同步处理委托。</param>
        void Unsubscribe<TCommand>(Action<TCommand> action) where TCommand : Command;

        /// <summary>
        /// 取消指定异步委托的订阅。
        /// </summary>
        /// <typeparam name="TCommand">命令类型。</typeparam>
        /// <param name="asyncAction">异步处理委托。</param>
        void Unsubscribe<TCommand>(Func<TCommand, Task> asyncAction) where TCommand : Command;

        /// <summary>
        /// 同步发布一个命令或查询。
        /// </summary>
        /// <typeparam name="TCommand">命令类型。</typeparam>
        /// <param name="command">待发布的命令。</param>
        void Publish<TCommand>(TCommand command) where TCommand : Command;

        /// <summary>
        /// 同步执行查询并返回结果。
        /// </summary>
        /// <typeparam name="TResponse">结果类型。</typeparam>
        /// <param name="query">查询对象。</param>
        /// <returns>查询结果。</returns>
        TResponse Query<TResponse>(Query<TResponse> query);

        /// <summary>
        /// 异步发布一个命令或查询。
        /// </summary>
        /// <typeparam name="TCommand">命令类型。</typeparam>
        /// <param name="command">待发布的命令。</param>
        /// <returns>异步任务。</returns>
        Task PublishAsync<TCommand>(TCommand command) where TCommand : Command;

        /// <summary>
        /// 异步执行查询并返回结果。
        /// </summary>
        /// <typeparam name="TResponse">结果类型。</typeparam>
        /// <param name="query">查询对象。</param>
        /// <returns>查询结果。</returns>
        Task<TResponse> QueryAsync<TResponse>(Query<TResponse> query);

        /// <summary>
        /// 注册实例处理器的服务解析回调。
        /// 自动发现的实例处理器会通过该回调创建对象后再执行方法。
        /// </summary>
        /// <param name="serviceHandlerAction">服务解析与执行回调。</param>
        void RegisterServiceHandlerAction(Action<Type, Action<object>> serviceHandlerAction);
    }
}
