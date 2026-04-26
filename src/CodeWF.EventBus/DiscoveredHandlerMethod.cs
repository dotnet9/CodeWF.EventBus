using System;
using System.Reflection;

namespace CodeWF.EventBus
{
    /// <summary>
    /// 程序集扫描后登记的处理方法描述信息。
    /// 真正发布时，会先解析实例对象，再把这个方法绑定并执行。
    /// </summary>
    internal sealed class DiscoveredHandlerMethod
    {
        /// <summary>
        /// 处理器所在类型。
        /// </summary>
        public Type RecipientType { get; set; }

        /// <summary>
        /// 最终要绑定到实例上的方法。
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// 当前处理器的执行顺序。
        /// </summary>
        public int Order { get; set; }
    }
}
