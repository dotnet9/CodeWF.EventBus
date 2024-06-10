using System;

namespace CodeWF.EventBus
{
    /// <summary>  
    /// 表示一个自定义的事件处理器特性，用于标记某个方法为事件处理器，并指定其执行顺序。  
    /// </summary>  
    /// <remarks>  
    /// 此特性仅适用于方法。  
    /// </remarks>  
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandlerAttribute : Attribute
    {
        /// <summary>  
        /// 获取或设置事件处理器的执行顺序。  
        /// 数值越小，执行优先级越高。  
        /// </summary> 
        public int Order { get; set; }
    }
}