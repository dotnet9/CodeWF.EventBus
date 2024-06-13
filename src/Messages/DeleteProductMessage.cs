namespace Messages
{
    public class DeleteProductMessage : CodeWF.EventBus.Message
    {
        public string Id { get; }

        public DeleteProductMessage(object sender, string id) : base(sender)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"Delete product message ->Product ID：{Id}";
        }
    }
}