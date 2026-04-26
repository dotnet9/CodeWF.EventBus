using System;

namespace CodeWF.EventBus
{
    /// <summary>
    /// 运行时订阅项。
    /// 保存事件总线已经可直接执行的委托和关联元数据。
    /// </summary>
    internal sealed class SubscriptionEntry
    {
        /// <summary>
        /// 订阅者类型。
        /// </summary>
        public Type RecipientType { get; set; }

        /// <summary>
        /// 实际执行的委托。
        /// </summary>
        public Delegate Action { get; set; }

        /// <summary>
        /// 当前处理器的执行顺序。
        /// </summary>
        public int Order { get; set; }
    }
}
