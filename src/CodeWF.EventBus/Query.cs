namespace CodeWF.EventBus
{
    public abstract class Query<TResponse> : Command
    {
        public abstract TResponse Result { get; set; }
    }
}