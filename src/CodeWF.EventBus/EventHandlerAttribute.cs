using System;

namespace CodeWF.EventBus
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandlerAttribute : Attribute
    {
        public int Order { get; set; }
    }
}