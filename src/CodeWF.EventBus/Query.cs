namespace CodeWF.EventBus
{
    public abstract class Query<TResult> : Command
    {
        public abstract TResult Result { get; set; }
    }
}