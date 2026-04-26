using System;

namespace CodeWF.EventBus
{
    /// <summary>
    /// 标记一个方法为事件处理方法。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandlerAttribute : Attribute
    {
        /// <summary>
        /// 同一命令下多个处理器的执行顺序，值越小越先执行。
        /// </summary>
        public int Order { get; set; }
    }
}
