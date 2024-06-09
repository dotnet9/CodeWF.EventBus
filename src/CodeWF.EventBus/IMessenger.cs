using System;

namespace CodeWF.EventBus
{
    /// <summary>
    /// 消息传递接口，用于订阅、取消订阅和发布消息。
    /// </summary>
    public interface IMessenger
    {
        /// <summary>  
        /// 订阅特定类型的消息。  
        /// </summary>  
        /// <typeparam name="TMessage">要订阅的消息类型，必须是Message类的派生类。</typeparam>  
        /// <param name="recipient">订阅者对象。</param>  
        /// <param name="action">当接收到消息时调用的操作。</param>  
        /// <param name="threadOption">消息处理线程选项，默认为PublisherThread。</param>  
        /// <param name="tag">用于标识订阅的标签，可以为null。</param>  
        void Subscribe<TMessage>(object recipient, Action<TMessage> action,
            ThreadOption threadOption = ThreadOption.PublisherThread, string tag = null) where TMessage : Message;

        /// <summary>  
        /// 订阅所有类型的消息（无特定类型）。  
        /// 注意：此方法的具体实现和行为取决于实现类，可能不支持或具有不同的行为。  
        /// </summary>  
        /// <param name="recipient">订阅者对象。</param>  
        void Subscribe(object recipient);

        /// <summary>  
        /// 取消订阅特定类型的消息。  
        /// </summary>  
        /// <typeparam name="TMessage">要取消订阅的消息类型，必须是Message类的派生类。</typeparam>  
        /// <param name="recipient">订阅者对象。</param>  
        /// <param name="action">要取消订阅的操作，如果为null，则取消该订阅者所有对TMessage类型的订阅。</param> 
        void Unsubscribe<TMessage>(object recipient, Action<TMessage> action = null) where TMessage : Message;

        /// <summary>  
        /// 发布消息。  
        /// </summary>  
        /// <typeparam name="TMessage">要发布的消息类型，必须是Message类的派生类。</typeparam>  
        /// <param name="sender">消息的发送者。</param>  
        /// <param name="message">要发布的消息。</param>  
        /// <param name="tag">用于标识发布的标签，可以为null。</param>  
        void Publish<TMessage>(object sender, TMessage message, string tag = null) where TMessage : Message;
    }
}