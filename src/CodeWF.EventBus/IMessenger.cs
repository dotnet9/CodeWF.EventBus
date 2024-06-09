using System;

namespace CodeWF.EventBus
{
    /// <summary>
    /// 消息传递接口，用于订阅、取消订阅和发布消息
    /// </summary>
    public interface IMessenger
    {
        /// <summary>  
        /// 订阅所有类型的消息（无特定类型）
        /// 注意：消息处理方法需要添加EventHandlerAttribute特性，并且参数为消息类型
        /// </summary>  
        /// <param name="recipient">订阅者对象</param>
        void Subscribe(object recipient);

        /// <summary>  
        /// 订阅特定类型的消息
        /// </summary>  
        /// <typeparam name="TMessage">要订阅的消息类型，必须是Message类的派生类</typeparam>  
        /// <param name="recipient">订阅者对象</param>  
        /// <param name="action">当接收到消息时调用的操作</param>  
        void Subscribe<TMessage>(object recipient, Action<TMessage> action) where TMessage : Message;

        /// <summary>  
        /// 取消订阅指定对象上的所有事件处理函数  
        /// </summary>  
        /// <param name="recipient">订阅者对象。</param>  
        void Unsubscribe(object recipient);

        /// <summary>  
        /// 取消订阅指定对象上的特定事件处理函数
        /// </summary>  
        /// <typeparam name="TMessage">要取消订阅的消息类型，必须是Message类的派生类</typeparam>  
        /// <param name="recipient">订阅者对象</param>  
        /// <param name="action">要取消订阅的操作，如果为null，则取消该订阅者所有对TMessage类型的订阅</param> 
        void Unsubscribe<TMessage>(object recipient, Action<TMessage> action = null) where TMessage : Message;

        /// <summary>  
        /// 发布消息
        /// </summary>  
        /// <typeparam name="TMessage">要发布的消息类型，必须是Message类的派生类</typeparam>  
        /// <param name="sender">消息的发送者</param>  
        /// <param name="message">要发布的消息</param>  
        void Publish<TMessage>(object sender, TMessage message) where TMessage : Message;
    }
}