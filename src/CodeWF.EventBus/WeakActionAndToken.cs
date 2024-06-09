using System;

namespace CodeWF.EventBus
{
    /// <summary>  
    /// 用于存储一个可以执行的委托和相关的接收者
    /// </summary> 
    public class WeakActionAndToken
    {
        /// <summary>  
        /// 获取或设置接收者对象
        /// </summary>
        public object Recipient { get; set; }

        /// <summary>  
        /// 获取或设置要执行的委托。  
        /// </summary> 
        public Delegate Action { get; set; }

        /// <summary>  
        /// 获取或设置动作的执行顺序。  
        /// </summary> 
        public int Order { get; set; }

        /// <summary>  
        /// 使用给定的消息对象执行动作
        /// </summary>  
        /// <typeparam name="TMessage">消息类型，必须继承自Message类</typeparam>  
        /// <param name="message">要传递给动作的消息对象</param>  
        /// <exception cref="InvalidCastException">如果Action委托的类型与Action&lt;TMessage&gt;不匹配。</exception>
        public void ExecuteWithObject<TMessage>(TMessage message) where TMessage : Message
        {
            if (Action is Action<TMessage> factAction) factAction.Invoke(message);
        }
    }
}