namespace CodeWF.EventBus
{
    /// <summary>
    /// 查询消息基类。
    /// 继承自 <see cref="Command"/>，并要求处理器在执行后写回结果。
    /// </summary>
    /// <typeparam name="TResponse">查询结果类型。</typeparam>
    public abstract class Query<TResponse> : Command
    {
        /// <summary>
        /// 查询处理完成后写回的结果。
        /// </summary>
        public abstract TResponse Result { get; set; }
    }
}
