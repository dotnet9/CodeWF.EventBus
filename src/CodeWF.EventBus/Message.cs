using System;

namespace CodeWF.EventBus
{
    /// <summary>  
    /// 消息基类 
    /// </summary>  
    public abstract class Message
    {
        /// <summary>  
        /// 发送消息的对象 
        /// </summary> 
        public object Sender { get; }

        /// <summary>  
        /// 初始化消息实例，并设置发送者 
        /// </summary>  
        /// <param name="sender">发送消息的对象，不能为空</param>  
        /// <exception cref="ArgumentNullException">如果sender为null，则抛出此异常</exception> 
        protected Message(object sender)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }
    }
}