using System;

namespace CodeWF.EventBus
{
    /// <summary>
    /// 标记一个类可以被 IOC 集成层自动发现为事件处理器容器。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EventAttribute : Attribute
    {
    }
}
